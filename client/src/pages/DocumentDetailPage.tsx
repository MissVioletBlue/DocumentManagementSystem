import { useEffect, useState } from 'react'
import { RouterLink } from '../router'
import { type DocumentDetails, fetchDocument } from '../services/documents'

type FetchState = 'idle' | 'loading' | 'success' | 'error'

interface DocumentDetailPageProps {
    documentId: number
}

const DocumentDetailPage = ({ documentId }: DocumentDetailPageProps) => {
    const [status, setStatus] = useState<FetchState>('idle')
    const [error, setError] = useState<string | null>(null)
    const [document, setDocument] = useState<DocumentDetails | null>(null)

    useEffect(() => {
        setStatus('loading')
        setError(null)
        setDocument(null)

        let cancelled = false
        fetchDocument(documentId)
            .then((result) => {
                if (!cancelled) {
                    setDocument(result)
                    setStatus('success')
                }
            })
            .catch((err: unknown) => {
                if (!cancelled) {
                    console.error(err)
                    setError(err instanceof Error ? err.message : 'Unable to load the document')
                    setStatus('error')
                }
            })

        return () => {
            cancelled = true
        }
    }, [documentId])

    return (
        <section className="detail">
            <div className="detail__header">
                <div>
                    <h1 className="page-title">Document Details</h1>
                    <p className="page-subtitle">View the metadata ingested for this document.</p>
                </div>
                <RouterLink className="back-link" to="/documents">
                    ← Back to documents
                </RouterLink>
            </div>

            {status === 'loading' ? (
                <p className="loading">Loading document…</p>
            ) : status === 'error' ? (
                <div role="alert" className="error">
                    <p>{error}</p>
                </div>
            ) : document ? (
                <article className="document-card" aria-live="polite">
                    <dl className="document-properties">
                        <div>
                            <dt>Title</dt>
                            <dd>{document.documentTitle}</dd>
                        </div>
                        <div>
                            <dt>Unique Identifier</dt>
                            <dd>{document.uniqueIdentifier}</dd>
                        </div>
                        <div>
                            <dt>Author</dt>
                            <dd>{document.documentAuthor || '—'}</dd>
                        </div>
                        <div>
                            <dt>Location</dt>
                            <dd>{document.documentLocation || '—'}</dd>
                        </div>
                        <div>
                            <dt>Tags</dt>
                            <dd>
                                {document.documentTags.length ? (
                                    <ul className="tag-list">
                                        {document.documentTags.map((tag) => (
                                            <li key={tag}>{tag}</li>
                                        ))}
                                    </ul>
                                ) : (
                                    '—'
                                )}
                            </dd>
                        </div>
                    </dl>
                </article>
            ) : null}
        </section>
    )
}

export default DocumentDetailPage