import React, { useState, useEffect } from "react";
import { createRoot } from "react-dom/client";

const API = "/api/salaries";
const EMPLOYEES_API = "/api/employees";

const EMPTY = {
    employeeId: "",
    amount: "",
    currency: "USD",
    effectiveDate: new Date().toISOString().slice(0, 10),
    note: "",
};

function SalaryCrud() {
    const [salaries, setSalaries] = useState([]);
    const [employees, setEmployees] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    const [form, setForm] = useState(EMPTY);
    const [editingId, setEditingId] = useState(null);

    const [page, setPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const pageSize = 5;

    const loadSalaries = (goToPage = page) => {
        setLoading(true);
        fetch(`${API}?page=${goToPage}&pageSize=${pageSize}`)
            .then((res) => res.json())
            .then((data) => {
                setSalaries(data.items);
                setTotalPages(data.totalPages || 1);
                setPage(data.page);
            })
            .catch((err) => setError(err.message))
            .finally(() => setLoading(false));
    };

    const loadEmployees = () => {
        fetch(`${EMPLOYEES_API}?page=1&pageSize=100`)
            .then((res) => res.json())
            .then((data) => setEmployees(data.items || []))
            .catch(() => setEmployees([]));
    };

    useEffect(() => loadSalaries(page), [page]);
    useEffect(() => loadEmployees(), []);

    const [events, setEvents] = useState([]);
    useEffect(() => {
        const fetchEvents = () =>
            fetch("/api/events?topic=salary")
                .then((res) => res.json())
                .then((data) => setEvents(data))
                .catch(() => {});

        fetchEvents();
        const timer = setInterval(fetchEvents, 2000);
        return () => clearInterval(timer);
    }, []);

    const onChange = (e) =>
        setForm((current) => ({ ...current, [e.target.name]: e.target.value }));

    const resetForm = () => {
        setForm({ ...EMPTY, effectiveDate: new Date().toISOString().slice(0, 10) });
        setEditingId(null);
    };

    const save = (e) => {
        e.preventDefault();
        if (!form.employeeId || !form.amount || !form.currency || !form.effectiveDate) return;

        const payload = {
            employeeId: Number(form.employeeId),
            amount: Number(form.amount) || 0,
            currency: form.currency,
            effectiveDate: form.effectiveDate,
            note: form.note,
        };

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
                loadSalaries(1);
            })
            .catch((err) => setError(err.message));
    };

    const startEdit = (salary) => {
        setEditingId(salary.id);
        setForm({
            employeeId: String(salary.employeeId),
            amount: String(salary.amount),
            currency: salary.currency,
            effectiveDate: salary.effectiveDate?.split("T")[0] || new Date().toISOString().slice(0, 10),
            note: salary.note || "",
        });
    };

    const remove = (id) => {
        if (!window.confirm("Delete this salary record?")) return;
        fetch(`${API}/${id}`, { method: "DELETE" })
            .then((res) => {
                if (!res.ok) throw new Error("HTTP " + res.status);
                loadSalaries();
            })
            .catch((err) => setError(err.message));
    };

    const employeeName = (employeeId) => {
        const employee = employees.find((emp) => emp.id === employeeId);
        return employee ? employee.name : `#${employeeId}`;
    };

    return (
        <div style={{ maxWidth: 820 }}>
            {error && <div className="alert alert-danger">Error: {error}</div>}

            <div className="card mb-4 border-success">
                <div className="card-header bg-success text-white">
                    📡 Live RabbitMQ Events (consumed)
                </div>
                <div className="card-body p-2" style={{ maxHeight: 180, overflowY: "auto" }}>
                 
                    {events.length === 0 ? (
                        <p className="text-muted m-2 mb-0">
                            No events yet — add/edit/delete a salary record below and watch them appear here.
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

            <form onSubmit={save} className="card card-body mb-4">
                <h5>{editingId ? `Edit salary #${editingId}` : "Add a salary record"}</h5>
                <div className="row g-2">
                    <div className="col-md-3">
                        <select
                            className="form-select"
                            name="employeeId"
                            value={form.employeeId}
                            onChange={onChange}
                        >
                            <option value="">Select employee</option>
                            {employees.map((emp) => (
                                <option key={emp.id} value={emp.id}>
                                    {emp.name} ({emp.email})
                                </option>
                            ))}
                        </select>
                    </div>
                    <div className="col-md-2">
                        <input
                            className="form-control"
                            name="amount"
                            type="number"
                            placeholder="Amount"
                            value={form.amount}
                            onChange={onChange}
                        />
                    </div>
                    <div className="col-md-2">
                        <input
                            className="form-control"
                            name="currency"
                            placeholder="Currency"
                            value={form.currency}
                            onChange={onChange}
                        />
                    </div>
                    <div className="col-md-2">
                        <input
                            className="form-control"
                            name="effectiveDate"
                            type="date"
                            value={form.effectiveDate}
                            onChange={onChange}
                        />
                    </div>
                    <div className="col-md-2">
                        <input
                            className="form-control"
                            name="note"
                            placeholder="Note"
                            value={form.note}
                            onChange={onChange}
                        />
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

            {loading ? (
                <p>Loading…</p>
            ) : (
                <table className="table table-striped align-middle">
                    <thead>
                        <tr>
                            <th>Id</th>
                            <th>Employee</th>
                            <th className="text-end">Amount</th>
                            <th>Currency</th>
                            <th>Effective Date</th>
                            <th>Note</th>
                            <th className="text-end">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {salaries.length === 0 && (
                            <tr><td colSpan="7" className="text-muted">No salary records.</td></tr>
                        )}
                        {salaries.map((salary) => (
                            <tr key={salary.id}>
                                <td>{salary.id}</td>
                                <td>{employeeName(salary.employeeId)}</td>
                                <td className="text-end">₹{salary.amount}</td>
                                <td>{salary.currency}</td>
                                <td>{salary.effectiveDate?.split("T")[0]}</td>
                                <td>{salary.note}</td>
                                <td className="text-end">
                                    <button className="btn btn-sm btn-outline-primary me-2" onClick={() => startEdit(salary)}>
                                        Edit
                                    </button>
                                    <button className="btn btn-sm btn-outline-danger" onClick={() => remove(salary.id)}>
                                        Delete
                                    </button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}

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

createRoot(document.getElementById("salaries-root")).render(<SalaryCrud />);
