import React, { useState, useEffect } from "react";
import { createRoot } from "react-dom/client";

function TaskApp() {
    const [tasks, setTasks] = useState([]);
    const [text, setText] = useState("");
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    // Fetch the seed tasks from the ASP.NET API once on mount.
    useEffect(() => {
        fetch("/api/tasks")
            .then((res) => {
                if (!res.ok) throw new Error("HTTP " + res.status);
                return res.json();
            })
            .then((data) => setTasks(data))
            .catch((err) => setError(err.message))
            .finally(() => setLoading(false));
    }, []);

    const add = () => {
        const label = text.trim();
        if (!label) return;
        setTasks((t) => [...t, { id: Date.now(), label, done: false }]);
        setText("");
    };

    const toggle = (id) =>
        setTasks((t) =>
            t.map((task) =>
                task.id === id ? { ...task, done: !task.done } : task
            )
        );

    const remove = (id) =>
        setTasks((t) => t.filter((task) => task.id !== id));

    if (loading) return <p>Loading tasks…</p>;
    if (error) return <p className="text-danger">Failed to load: {error}</p>;

    const remaining = tasks.filter((t) => !t.done).length;

    return (
        <div className="card" style={{ maxWidth: 520 }}>
            <div className="card-body">
                <div className="input-group mb-3">
                    <input
                        className="form-control"
                        value={text}
                        placeholder="Add a task…"
                        onChange={(e) => setText(e.target.value)}
                        onKeyDown={(e) => e.key === "Enter" && add()}
                    />
                    <button className="btn btn-primary" onClick={add}>
                        Add
                    </button>
                </div>

                <ul className="list-group">
                    {tasks.length === 0 && (
                        <li className="list-group-item text-muted">No tasks yet.</li>
                    )}
                    {tasks.map((task) => (
                        <li
                            key={task.id}
                            className="list-group-item d-flex align-items-center gap-2"
                        >
                            <input
                                type="checkbox"
                                className="form-check-input mt-0"
                                checked={task.done}
                                onChange={() => toggle(task.id)}
                            />
                            <span
                                className={
                                    "flex-grow-1" + (task.done? " text-decoration-line-through text-muted": "")
                                }
                            >
                                {task.label}
                            </span>
                            <button
                                className="btn btn-sm btn-outline-danger"
                                onClick={() => remove(task.id)}
                            >
                                ✕
                            </button>
                        </li>
                    ))}
                </ul>

                <p className="text-muted mt-3 mb-0">{remaining} remaining</p>
            </div>
        </div>
    );
}

createRoot(document.getElementById("react-root")).render(<TaskApp />);
