import {
    createContext,
    useCallback,
    useContext,
    useEffect,
    useMemo,
    useState,
    type AnchorHTMLAttributes,
    type ReactNode,
} from 'react'

interface NavigateOptions {
    replace?: boolean
}

interface RouterState {
    path: string
    navigate: (to: string, options?: NavigateOptions) => void
}

const defaultState: RouterState = {
    path: typeof window !== 'undefined' ? window.location.pathname + window.location.search : '/',
    // eslint-disable-next-line @typescript-eslint/no-empty-function
    navigate: () => {},
}

const RouterContext = createContext<RouterState>(defaultState)

export function RouterProvider({ children }: { children: ReactNode }) {
    const [path, setPath] = useState(() => window.location.pathname + window.location.search)

    useEffect(() => {
        const handlePopState = () => {
            setPath(window.location.pathname + window.location.search)
        }

        window.addEventListener('popstate', handlePopState)
        return () => window.removeEventListener('popstate', handlePopState)
    }, [])

    const navigate = useCallback(
        (to: string, options?: NavigateOptions) => {
            const target = to.startsWith('/') ? to : `/${to}`

            if (target === path) {
                return
            }

            if (options?.replace) {
                window.history.replaceState({}, '', target)
            } else {
                window.history.pushState({}, '', target)
            }

            setPath(window.location.pathname + window.location.search)
        },
        [path],
    )

    const value = useMemo(() => ({ path, navigate }), [path, navigate])

    return <RouterContext.Provider value={value}>{children}</RouterContext.Provider>
}

export function useRouter() {
    return useContext(RouterContext)
}

interface LinkProps extends Omit<AnchorHTMLAttributes<HTMLAnchorElement>, 'href' | 'onClick'> {
    to: string
    replace?: boolean
}

export function RouterLink({ to, replace, children, ...rest }: LinkProps) {
    const { navigate } = useRouter()

    const handleClick = useCallback(
        (event: React.MouseEvent<HTMLAnchorElement>) => {
            if (
                event.defaultPrevented ||
                event.button !== 0 ||
                event.metaKey ||
                event.altKey ||
                event.ctrlKey ||
                event.shiftKey ||
                rest.target === '_blank'
            ) {
                return
            }

            event.preventDefault()
            navigate(to, { replace })
        },
        [navigate, replace, rest.target, to],
    )

    return (
        <a href={to} onClick={handleClick} {...rest}>
            {children}
        </a>
    )
}