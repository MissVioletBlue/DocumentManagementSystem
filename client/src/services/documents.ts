const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? '/api').replace(/\/$/, '')

const jsonHeaders: HeadersInit = {
    Accept: 'application/json',
}

function buildUrl(path: string): string {
    return `${API_BASE_URL}${path.startsWith('/') ? path : `/${path}`}`
}

async function parseJson<T>(response: Response): Promise<T> {
    const contentType = response.headers.get('content-type') ?? ''
    if (!contentType.includes('application/json')) {
        throw new Error('The server returned an unexpected response.')
    }
    return (await response.json()) as T
}

export interface DocumentSummary {
    uniqueIdentifier: number
    documentTitle: string
    documentAuthor?: string | null
    documentLocation?: string | null
    documentTags: string[]
}

export interface DocumentDetails extends DocumentSummary {}

export async function searchDocuments(query?: string): Promise<DocumentSummary[]> {
    const searchQuery = query?.trim() ?? ''
    const queryString = searchQuery ? `?q=${encodeURIComponent(searchQuery)}` : ''
    const response = await fetch(`${buildUrl('/documents')}${queryString}`, {
        headers: jsonHeaders,
    })

    if (!response.ok) {
        throw new Error(`Failed to load documents (${response.status})`)
    }

    const data = await parseJson<DocumentSummary[]>(response)
    return data.map((doc) => ({
        ...doc,
        documentTags: doc.documentTags ?? [],
    }))
}

export async function fetchDocument(id: number): Promise<DocumentDetails> {
    const response = await fetch(buildUrl(`/documents/${id}`), {
        headers: jsonHeaders,
    })

    if (response.status === 404) {
        throw new Error('Document not found')
    }

    if (!response.ok) {
        throw new Error(`Failed to load document (${response.status})`)
    }

    const data = await parseJson<DocumentDetails>(response)
    return {
        ...data,
        documentTags: data.documentTags ?? [],
    }
}