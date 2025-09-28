import { useEffect, useMemo, type ReactNode } from 'react'
import DashboardPage from './pages/DashboardPage'
import DocumentDetailPage from './pages/DocumentDetailPage'
import { RouterLink, RouterProvider, useRouter } from './router'
import './App.css'

interface AppLayoutProps {
    activePath: string
    children: ReactNode
}

function AppLayout({ activePath, children }: AppLayoutProps) {
    const isDocumentRoute = activePath.startsWith('/documents')

  return (
      <div className="app-shell">
          <header className="app-header">
              <div className="brand">
          <span className="brand__logo" aria-hidden="true">
            📄
          </span>
                  <div>
                      <p className="brand__title">Document Management</p>
                      <p className="brand__subtitle">Dashboard</p>
                  </div>
              </div>
              <nav className="app-nav">
                  <RouterLink to="/documents" className={isDocumentRoute ? 'nav-link nav-link--active' : 'nav-link'}>
                      Documents
                  </RouterLink>
                  <a className="nav-link" href="/paperless/" rel="noreferrer">
                      Paperless
                  </a>
              </nav>
          </header>
          <main className="app-main">{children}</main>
          <footer className="app-footer">Powered by DMS SWEN3</footer>
      </div>
  )
}

function AppRoutes() {
    const { path, navigate } = useRouter()

    useEffect(() => {
        if (path === '/' || path === '') {
            navigate('/documents', { replace: true })
        }
    }, [navigate, path])

    const detailId = useMemo(() => {
        const match = path.match(/^\/documents\/(\d+)/)
        if (!match) return null

        const id = Number.parseInt(match[1] ?? '', 10)
        return Number.isNaN(id) ? null : id
    }, [path])

    const isDetailRoute = path.startsWith('/documents/')

    let content: ReactNode

    if (isDetailRoute && detailId === null) {
        content = (
            <div className="detail">
                <div className="detail__header">
                    <div>
                        <h1 className="page-title">Document not found</h1>
                        <p className="page-subtitle">The requested document identifier is invalid.</p>
                    </div>
                    <RouterLink className="back-link" to="/documents" replace>
                        ← Back to documents
                    </RouterLink>
                </div>
      </div>
        )
    } else if (detailId !== null) {
        content = <DocumentDetailPage documentId={detailId} />
    } else if (path.startsWith('/documents')) {
        const url = new URL(path, window.location.origin)
        const query = url.searchParams.get('q') ?? ''
        content = <DashboardPage query={query} />
    } else {
        content = (
            <div className="detail">
                <div className="detail__header">
                    <div>
                        <h1 className="page-title">Page not found</h1>
                        <p className="page-subtitle">The page you are looking for does not exist.</p>
                    </div>
                    <RouterLink className="back-link" to="/documents" replace>
                        ← Back to documents
                    </RouterLink>
                </div>
      </div>
        )
    }

    return <AppLayout activePath={path}>{content}</AppLayout>
}   

function App() {
    return (
        <RouterProvider>
            <AppRoutes />
        </RouterProvider>
  )
}

export default App
