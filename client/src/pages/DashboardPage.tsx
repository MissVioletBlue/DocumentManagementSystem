import { type FormEvent, useEffect, useMemo, useState } from 'react'
import { RouterLink, useRouter } from '../router'
import { type DocumentSummary, searchDocuments } from '../services/documents'

type FetchState = 'idle' | 'loading' | 'success' | 'error'

interface DashboardPageProps {
    query: string
}

const DashboardPage = ({ query }: DashboardPageProps) => {
    const { navigate } = useRouter()
    const queryFromUrl = query

    const [searchInput, setSearchInput] = useState(queryFromUrl)
    const [documents, setDocuments] = useState<DocumentSummary[]>([])
    const [status, setStatus] = useState<FetchState>('idle')
    const [error, setError] = useState<string | null>(null)
    const [reloadToken, setReloadToken] = useState(0)

    useEffect(() => {
        setSearchInput(queryFromUrl)
    }, [queryFromUrl])

    useEffect(() => {
        setStatus('loading')
        setError(null)
        setDocuments([])

        let cancelled = false
        searchDocuments(queryFromUrl)
            .then((result) => {
                if (!cancelled) {
                    setDocuments(result)
                    setStatus('success')
                }
            })
            .catch((err: unknown) => {
                if (!cancelled) {
                    console.error(err)
                    setError(err instanceof Error ? err.message : 'An unexpected error occurred')
                    setStatus('error')
                }
            })

        return () => {
            cancelled = true
        }
    }, [queryFromUrl, reloadToken])

    const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
        event.preventDefault()
        const trimmed = searchInput.trim()
        navigate(trimmed ? `/documents?q=${encodeURIComponent(trimmed)}` : '/documents')
    }

    const documentCountLabel = useMemo(() => {
        if (status === 'loading') return 'Loading documents…'
        if (status === 'error') return 'Unable to load documents'
        if (!documents.length) return 'No documents found'
        return `${documents.length} document${documents.length === 1 ? '' : 's'}`
    }, [documents.length, status])

    return (
        <section className="dashboard">
            <div className="dashboard__header">
                <div>
                    <h1 className="page-title">Documents</h1>
                    <p className="page-subtitle">Search and review documents ingested through the Software Engineering 3 DMS API.</p>
                </div>
                <form className="search" onSubmit={handleSubmit}>
                    <label className="search__label" htmlFor="query">
                        Search
                    </label>
                    <div className="search__field">
                        <input
                            id="query"
                            name="query"
                            type="search"
                            placeholder="Search by title, author, or tags"
                            value={searchInput}
                            onChange={(event) => setSearchInput(event.target.value)}
                        />
                        <button type="submit">Apply</button>
                    </div>
                </form>
            </div>

            <div className="dashboard__body">
                <p className="document-count" aria-live="polite">
                    {documentCountLabel}
                </p>

                {status === 'error' ? (
                    <div role="alert" className="error">
                        <p>{error}</p>
                        <button type="button" onClick={() => setReloadToken((token) => token + 1)}>
                            Retry
                        </button>
                    </div>
                ) : (
                    <div className="table-wrapper" role="region" aria-live="polite">
                        <table className="document-table">
                            <thead>
                            <tr>
                                <th scope="col">Title</th>
                                <th scope="col">Author</th>
                                <th scope="col">Location</th>
                                <th scope="col">Tags</th>
                            </tr>
                            </thead>
                            <tbody>
                            {status === 'loading' ? (
                                <tr>
                                    <td colSpan={4} className="empty">Loading…</td>
                                </tr>
                            ) : documents.length ? (
                                documents.map((document) => (
                                    <tr key={document.uniqueIdentifier}>
                                        <th scope="row">
                                            <RouterLink to={`/documents/${document.uniqueIdentifier}`}>{document.documentTitle}</RouterLink>
                                        </th>
                                        <td>{document.documentAuthor || '—'}</td>
                                        <td>{document.documentLocation || '—'}</td>
                                        <td>
                                            {document.documentTags.length ? (
                                                <ul className="tag-list">
                                                    {document.documentTags.map((tag) => (
                                                        <li key={tag}>{tag}</li>
                                                    ))}
                                                </ul>
                                            ) : (
                                                '—'
                                            )}
                                        </td>
                                    </tr>
                                ))
                            ) : (
                                <tr>
                                    <td colSpan={4} className="empty">
                                        No documents match your search criteria.
                                    </td>
                                </tr>
                            )}
                            </tbody>
                        </table>
                    </div>
                )}
            </div>
        </section>
    )
}

export default DashboardPage