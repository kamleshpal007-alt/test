import React, { useState, useEffect } from "react";
import { createRoot } from "react-dom/client";

const API = "/api/departments";

const EMPTY = { name: "", code: "", description: "" };

function DepartmentCrud() {
    const [departments, setDepartments] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    const [form, setForm] = useState(EMPTY);
    const [editingId, setEditingId] = useState(null); // null = creating

    // ---- Pagination state ----
    const [page, setPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const pageSize = 5;

    // ---- READ one PAGE of departments from the database ----
    const load = (goToPage = page) => {
        setLoading(true);
        fetch(`${API}?page=${goToPage}&pageSize=${pageSize}`)
            .then((res) => res.json())
            .then((data) => {
                setDepartments(data.items);        // just this page's rows
                setTotalPages(data.totalPages || 1);
                setPage(data.page);
            })
            .catch((err) => setError(err.message))
            .finally(() => setLoading(false));
    };

    // Reload whenever the page number changes.
    useEffect(() => load(page), [page]);

    // ---- Live RabbitMQ events: poll /api/events every 2 seconds ----
    const [events, setEvents] = useState([]);
    useEffect(() => {
        const fetchEvents = () =>
            fetch("/api/events?topic=department")
                .then((res) => res.json())
                .then((data) => setEvents(data))
                .catch(() => {});
        fetchEvents();                              // once immediately
        const timer = setInterval(fetchEvents, 2000); // then every 2s
        return () => clearInterval(timer);          // cleanup on unmount
    }, []);

    // Update one field of the form object as the user types.
    const onChange = (e) =>
        setForm((f) => ({ ...f, [e.target.name]: e.target.value }));

    const resetForm = () => {
        setForm(EMPTY);
        setEditingId(null);
    };

    // ---- CREATE or UPDATE ----
    const save = (e) => {
        e.preventDefault();
        if (!form.name.trim() || !form.code.trim()) return;

        const opts = {
            method: editingId ? "PUT" : "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(form),
        };
        const url = editingId ? `${API}/${editingId}` : API;

        fetch(url, opts)
            .then(async (res) => {
                if (!res.ok) {
                    // The API returns 409 with a plain-text message on duplicate code.
                    const msg = await res.text();
                    throw new Error(msg || "HTTP " + res.status);
                }
                return res.json();
            })
            .then(() => {
                setError(null);
                resetForm();
                load();
            })
            .catch((err) => setError(err.message));
    };

    const startEdit = (dept) => {
        setEditingId(dept.id);
        setForm({
            name: dept.name,
            code: dept.code,
            description: dept.description,
        });
    };

    // ---- DELETE ----
    const remove = (id) => {
        if (!window.confirm("Delete this department?")) return;
        fetch(`${API}/${id}`, { method: "DELETE" })
            .then((res) => {
                if (!res.ok) throw new Error("HTTP " + res.status);
                load();
            })
            .catch((err) => setError(err.message));
    };

    return (
        <div style={{ maxWidth: 820 }}>
            {error && <div className="alert alert-danger">Error: {error}</div>}

            {/* ---- Live RabbitMQ events (consumed by the background service) ---- */}
            <div className="card mb-4 border-success">
                <div className="card-header bg-success text-white">
                    📡 Live RabbitMQ Events (consumed)
                </div>
                <div className="card-body p-2" style={{ maxHeight: 180, overflowY: "auto" }}>
                    {events.length === 0 ? (
                        <p className="text-muted m-2 mb-0">
                            No events yet — add/edit/delete a department below and watch them appear here.
                        </p>
                    ) : (
                        <ul className="list-group list-group-flush">
                            {events.map((ev, i) => (
                                <li key={i} className="list-group-item py-1 d-flex gap-2">
                                    <span className="badge bg-secondary">{ev.time}</span>
                                    <code className="small">{ev.message}</code>
                                </li>
                            ))}
                        </ul>
                    )}
                </div>
            </div>

            {/* ---- Form: Create / Update ---- */}
            <form onSubmit={save} className="card card-body mb-4">
                <h5>{editingId ? `Edit department #${editingId}` : "Add a department"}</h5>
                <div className="row g-2">
                    <div className="col-md-4">
                        <input className="form-control" name="name" placeholder="Name"
                            value={form.name} onChange={onChange} />
                    </div>
                    <div className="col-md-2">
                        <input className="form-control" name="code" placeholder="Code"
                            value={form.code} onChange={onChange} />
                    </div>
                    <div className="col-md-4">
                        <input className="form-control" name="description" placeholder="Description"
                            value={form.description} onChange={onChange} />
                    </div>
                    <div className="col-md-2 d-grid">
                        <button type="submit" className="btn btn-primary">
                            {editingId ? "Save" : "Add"}
                        </button>
                    </div>
                </div>
                {editingId && (
                    <div className="mt-2">
                        <button type="button" className="btn btn-secondary btn-sm" onClick={resetForm}>
                            Cancel
                        </button>
                    </div>
                )}
            </form>

            {/* ---- Table: Read ---- */}
            {loading ? (
                <p>Loading…</p>
            ) : (
                <table className="table table-striped align-middle">
                    <thead>
                        <tr>
                            <th>Id</th>
                            <th>Name</th>
                            <th>Code</th>
                            <th>Description</th>
                            <th className="text-end">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {departments.length === 0 && (
                            <tr><td colSpan="5" className="text-muted">No departments.</td></tr>
                        )}
                        {departments.map((dept) => (
                            <tr key={dept.id}>
                                <td>{dept.id}</td>
                                <td>{dept.name}</td>
                                <td><code>{dept.code}</code></td>
                                <td>{dept.description}</td>
                                <td className="text-end">
                                    <button className="btn btn-sm btn-outline-primary me-2"
                                        onClick={() => startEdit(dept)}>Edit</button>
                                    <button className="btn btn-sm btn-outline-danger"
                                        onClick={() => remove(dept.id)}>Delete</button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}

            {/* ---- Pagination controls ---- */}
            {totalPages > 1 && (
                <nav className="d-flex justify-content-between align-items-center">
                    <button
                        className="btn btn-outline-secondary"
                        disabled={page <= 1}
                        onClick={() => setPage((p) => p - 1)}
                    >
                        ← Previous
                    </button>

                    <span className="text-muted">
                        Page <strong>{page}</strong> of <strong>{totalPages}</strong>
                    </span>

                    <button
                        className="btn btn-outline-secondary"
                        disabled={page >= totalPages}
                        onClick={() => setPage((p) => p + 1)}
                    >
                        Next →
                    </button>
                </nav>
            )}
        </div>
    );
}

createRoot(document.getElementById("departments-root")).render(<DepartmentCrud />);
