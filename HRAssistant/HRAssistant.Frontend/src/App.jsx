import { useState, useRef, useEffect, useCallback } from 'react'
import './App.css'

const API_BASE = import.meta.env.VITE_API_BASE ?? ''

const SUGGESTIONS = [
    'Give me user info',
    'What is the leave policy?',
    'How many leaves do I have left?',
    'I want to apply for leave from 20-06-2025 to 25-06-2025, reason: vacation',
]

function TypingDots() {
    return (
        <div className="typing-dots" aria-label="Agent is typing">
            <span /><span /><span />
        </div>
    )
}

function AgentBadge({ name }) {
    if (!name || name === '[Routing to agent...]') return null
    const colors = {
        SqlAgent: 'badge-blue',
        PolicyAgent: 'badge-teal',
        ActionAgent: 'badge-amber',
    }
    return (
        <span className={`badge ${colors[name] ?? 'badge-gray'}`}>
            {name === 'SqlAgent' && '?? '}
            {name === 'PolicyAgent' && '?? '}
            {name === 'ActionAgent' && '?? '}
            {name}
        </span>
    )
}

function Message({ msg }) {
    const isUser = msg.role === 'user'
    const isError = msg.isError

    return (
        <div className={`message-row ${isUser ? 'user' : 'agent'}`}>
            {!isUser && (
                <div className="avatar agent-avatar" aria-hidden="true">HR</div>
            )}
            <div className="bubble-wrapper">
                {!isUser && msg.agent && <AgentBadge name={msg.agent} />}
                <div className={`bubble ${isUser ? 'bubble-user' : 'bubble-agent'} ${isError ? 'bubble-error' : ''}`}>
                    {msg.isTyping ? <TypingDots /> : (
                        <span className="bubble-text">{msg.content}</span>
                    )}
                </div>
                {msg.timestamp && (
                    <span className="timestamp">{msg.timestamp}</span>
                )}
            </div>
            {isUser && (
                <div className="avatar user-avatar" aria-hidden="true">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                        <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" />
                        <circle cx="12" cy="7" r="4" />
                    </svg>
                </div>
            )}
        </div>
    )
}

export default function App() {
    const [messages, setMessages] = useState([
        {
            id: 'welcome',
            role: 'agent',
            content: 'Hi! I\'m your HR Assistant. I can help you with employee info, leave policies, and applying for leave. What can I help you with today?',
            agent: null,
            timestamp: now(),
        }
    ])
    const [input, setInput] = useState('')
    const [isStreaming, setIsStreaming] = useState(false)
    const bottomRef = useRef(null)
    const inputRef = useRef(null)
    const abortRef = useRef(null)

    function now() {
        return new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
    }

    useEffect(() => {
        bottomRef.current?.scrollIntoView({ behavior: 'smooth' })
    }, [messages])

    const sendMessage = useCallback(async (text) => {
        const userText = text ?? input.trim()
        if (!userText || isStreaming) return

        setInput('')
        setIsStreaming(true)

        const userMsg = {
            id: Date.now() + '-user',
            role: 'user',
            content: userText,
            timestamp: now(),
        }

        const typingId = Date.now() + '-typing'
        const agentId = Date.now() + '-agent'

        setMessages(prev => [...prev, userMsg, {
            id: typingId,
            role: 'agent',
            content: '',
            isTyping: true,
            agent: null,
            timestamp: null,
        }])

        abortRef.current = new AbortController()

        try {
            const url = `/agent/stream/123456789?message=${encodeURIComponent(userText)}`
            const res = await fetch(url, {
                signal: abortRef.current.signal,
                headers: { Accept: 'text/event-stream' },
            })

            if (!res.ok) {
                throw new Error(`Server error: ${res.status} ${res.statusText}`)
            }

            const reader = res.body.getReader()
            const decoder = new TextDecoder()
            let accumulated = ''
            let detectedAgent = null
            let buffer = ''

            setMessages(prev => prev.map(m =>
                m.id === typingId
                    ? { ...m, id: agentId, isTyping: false, content: '' }
                    : m
            ))

            while (true) {
                const { done, value } = await reader.read()
                if (done) break

                buffer += decoder.decode(value, { stream: true })
                const lines = buffer.split('\n')
                buffer = lines.pop()

                for (const line of lines) {
                    if (!line.startsWith('data: ')) continue
                    const data = line.slice(6)

                    if (data === '[DONE]') break

                    if (data.startsWith('[') && data.endsWith(']')) {
                        if (data.includes('Agent') && !data.includes('Routing')) {
                            detectedAgent = data.replace('[', '').replace(']', '').trim()
                        }
                        continue
                    }

                    accumulated += data

                    setMessages(prev => prev.map(m =>
                        m.id === agentId
                            ? { ...m, content: accumulated, agent: detectedAgent }
                            : m
                    ))
                }
            }

            setMessages(prev => prev.map(m =>
                m.id === agentId
                    ? { ...m, timestamp: now(), agent: detectedAgent }
                    : m
            ))

        } catch (err) {
            if (err.name === 'AbortError') {
                setMessages(prev => prev.filter(m => m.id !== typingId && m.id !== agentId))
            } else {
                setMessages(prev => prev.map(m =>
                    (m.id === typingId || m.id === agentId)
                        ? {
                            ...m,
                            id: agentId,
                            isTyping: false,
                            isError: true,
                            content: `Something went wrong: ${err.message}`,
                            timestamp: now(),
                        }
                        : m
                ))
            }
        } finally {
            setIsStreaming(false)
            abortRef.current = null
            inputRef.current?.focus()
        }
    }, [input, isStreaming])

    function handleKeyDown(e) {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault()
            sendMessage()
        }
    }

    function handleStop() {
        abortRef.current?.abort()
        setIsStreaming(false)
    }

    return (
        <div className="app">
            <header className="header">
                <div className="header-inner">
                    <div className="header-left">
                        <div className="logo" aria-hidden="true">
                            <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
                                <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
                                <circle cx="9" cy="7" r="4" />
                                <path d="M23 21v-2a4 4 0 0 0-3-3.87" />
                                <path d="M16 3.13a4 4 0 0 1 0 7.75" />
                            </svg>
                        </div>
                        <div>
                            <h1 className="header-title">HR Assistant</h1>
                            <p className="header-subtitle">Multi-agent · Powered by GPT-4o mini</p>
                        </div>
                    </div>
                    <div className="agents-legend">
                        <span className="badge badge-blue">?? SQL</span>
                        <span className="badge badge-teal">?? Policy</span>
                        <span className="badge badge-amber">?? Action</span>
                    </div>
                </div>
            </header>

            <main className="messages-area" role="log" aria-live="polite" aria-label="Chat messages">
                <div className="messages-inner">
                    {messages.map(msg => (
                        <Message key={msg.id} msg={msg} />
                    ))}
                    <div ref={bottomRef} />
                </div>
            </main>

            {messages.length === 1 && (
                <div className="suggestions">
                    <p className="suggestions-label">Try asking</p>
                    <div className="suggestions-grid">
                        {SUGGESTIONS.map(s => (
                            <button
                                key={s}
                                className="suggestion-chip"
                                onClick={() => sendMessage(s)}
                                disabled={isStreaming}
                            >
                                {s}
                            </button>
                        ))}
                    </div>
                </div>
            )}

            <footer className="input-area">
                <div className="input-inner">
                    <div className="input-box">
                        <textarea
                            ref={inputRef}
                            className="input-field"
                            value={input}
                            onChange={e => setInput(e.target.value)}
                            onKeyDown={handleKeyDown}
                            placeholder="Ask about leave, policies, or employee info…"
                            rows={1}
                            disabled={isStreaming}
                            aria-label="Message input"
                        />
                        {isStreaming ? (
                            <button className="send-btn stop-btn" onClick={handleStop} aria-label="Stop generating">
                                <svg width="16" height="16" viewBox="0 0 24 24" fill="currentColor">
                                    <rect x="4" y="4" width="16" height="16" rx="2" />
                                </svg>
                            </button>
                        ) : (
                            <button
                                className="send-btn"
                                onClick={() => sendMessage()}
                                disabled={!input.trim()}
                                aria-label="Send message"
                            >
                                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round">
                                    <line x1="22" y1="2" x2="11" y2="13" />
                                    <polygon points="22 2 15 22 11 13 2 9 22 2" />
                                </svg>
                            </button>
                        )}
                    </div>
                    <p className="input-hint">Press Enter to send · Shift+Enter for new line</p>
                </div>
            </footer>
        </div>
    )
}
