import React, { useState, useEffect } from "react";
import { createRoot } from "react-dom/client";

const API = "/api/employees";

const EMPTY = { name: "", email: "", department: "", salary: "" };

function EmployeeCrud() {
    const [employees, setEmployees] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    const [form, setForm] = useState(EMPTY);
    const [editingId, setEditingId] = useState(null); // null = creating

    // ---- Pagination state ----
    const [page, setPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const pageSize = 5;

    // ---- READ one PAGE of employees from the database ----
    const load = (goToPage = page) => {
        setLoading(true);
        fetch(`${API}?page=${goToPage}&pageSize=${pageSize}`)
            .then((res) => res.json())
            .then((data) => {
                setEmployees(data.items);          // just this page's rows
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
            fetch("/api/events")
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
        if (!form.name.trim() || !form.email.trim()) return;

        const payload = { ...form, salary: Number(form.salary) || 0 };
        const opts = {
            method: editingId ? "PUT" : "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload),
        };
        const url = editingId ? `${API}/${editingId}` : API;

        fetch(url, opts)
            .then((res) => {
                if (!res.ok) throw new Error("HTTP " + res.status);
                return res.json();
            })
            .then(() => {
                resetForm();
                load();
            })
            .catch((err) => setError(err.message));
    };

    const startEdit = (emp) => {
        setEditingId(emp.id);
        setForm({
            name: emp.name,
            email: emp.email,
            department: emp.department,
            salary: String(emp.salary),
        });
    };

    // ---- DELETE ----
    const remove = (id) => {
        if (!window.confirm("Delete this employee?")) return;
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
                            No events yet — add/edit/delete an employee below and watch them appear here.
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
                <h5>{editingId ? `Edit employee #${editingId}` : "Add an employee"}</h5>
                <div className="row g-2">
                    <div className="col-md-3">
                        <input className="form-control" name="name" placeholder="Name"
                            value={form.name} onChange={onChange} />
                    </div>
                    <div className="col-md-3">
                        <input className="form-control" name="email" placeholder="Email"
                            value={form.email} onChange={onChange} />
                    </div>
                    <div className="col-md-3">
                        <input className="form-control" name="department" placeholder="Department"
                            value={form.department} onChange={onChange} />
                    </div>
                    <div className="col-md-2">
                        <input className="form-control" name="salary" type="number" placeholder="Salary"
                            value={form.salary} onChange={onChange} />
                    </div>
                    <div className="col-md-1 d-grid">
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
                            <th>Email</th>
                            <th>Department</th>
                            <th className="text-end">Salary</th>
                            <th className="text-end">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {employees.length === 0 && (
                            <tr><td colSpan="6" className="text-muted">No employees.</td></tr>
                        )}
                        {employees.map((emp) => (
                            <tr key={emp.id}>
                                <td>{emp.id}</td>
                                <td>{emp.name}</td>
                                <td>{emp.email}</td>
                                <td>{emp.department}</td>
                                <td className="text-end">₹{emp.salary}</td>
                                <td className="text-end">
                                    <button className="btn btn-sm btn-outline-primary me-2"
                                        onClick={() => startEdit(emp)}>Edit</button>
                                    <button className="btn btn-sm btn-outline-danger"
                                        onClick={() => remove(emp.id)}>Delete</button>
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

createRoot(document.getElementById("employees-root")).render(<EmployeeCrud />);
