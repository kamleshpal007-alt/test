import React, { useState, useEffect } from "react";
import { createRoot } from "react-dom/client";

const API = "/api/products";

function ProductCrud() {
    const [products, setProducts] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    // Form fields (used for both Create and Update)
    const [name, setName] = useState("");
    const [price, setPrice] = useState("");
    const [editingId, setEditingId] = useState(null); // null = creating, number = editing

    // ---- READ: load all products from the server ----
    const load = () => {
        setLoading(true);
        fetch(API)
            .then((res) => res.json())
            .then((data) => setProducts(data))
            .catch((err) => setError(err.message))
            .finally(() => setLoading(false));
    };

    useEffect(load, []); // run once on first render

    const resetForm = () => {
        setName("");
        setPrice("");
        setEditingId(null);
    };

    // ---- CREATE or UPDATE (depending on editingId) ----
    const save = (e) => {
        e.preventDefault(); // stop the browser from reloading the page on submit
        if (!name.trim()) return;

        const body = JSON.stringify({ name, price: Number(price) || 0 });
        const opts = {
            method: editingId ? "PUT" : "POST",
            headers: { "Content-Type": "application/json" },
            body,
        };
        const url = editingId ? `${API}/${editingId}` : API;

        fetch(url, opts)
            .then((res) => {
                if (!res.ok) throw new Error("HTTP " + res.status);
                return res.json();
            })
            .then(() => {
                resetForm();
                load(); // re-fetch the fresh list from the server
            })
            .catch((err) => setError(err.message));
    };

    // ---- Begin editing a row: fill the form with its values ----
    const startEdit = (p) => {
        setEditingId(p.id);
        setName(p.name);
        setPrice(String(p.price));
    };

    // ---- DELETE ----
    const remove = (id) => {
        fetch(`${API}/${id}`, { method: "DELETE" })
            .then((res) => {
                if (!res.ok) throw new Error("HTTP " + res.status);
                load();
            })
            .catch((err) => setError(err.message));
    };

    return (
        <div style={{ maxWidth: 640 }}>
            {error && <div className="alert alert-danger">Error: {error}</div>}

            {/* ---- The form: Create (or Update when editing) ---- */}
            <form onSubmit={save} className="card card-body mb-4">
                <h5>{editingId ? `Edit product #${editingId}` : "Add a product"}</h5>
                <div className="row g-2">
                    <div className="col">
                        <input
                            className="form-control"
                            placeholder="Name"
                            value={name}
                            onChange={(e) => setName(e.target.value)}
                        />
                    </div>
                    <div className="col">
                        <input
                            className="form-control"
                            type="number"
                            placeholder="Price"
                            value={price}
                            onChange={(e) => setPrice(e.target.value)}
                        />
                    </div>
                    <div className="col-auto">
                        <button type="submit" className="btn btn-primary">
                            {editingId ? "Update" : "Add"}
                        </button>
                        {editingId && (
                            <button
                                type="button"
                                className="btn btn-secondary ms-2"
                                onClick={resetForm}
                            >
                                Cancel
                            </button>
                        )}
                    </div>
                </div>
            </form>

            {/* ---- The table: Read ---- */}
            {loading ? (
                <p>Loading…</p>
            ) : (
                <table className="table table-striped">
                    <thead>
                        <tr>
                            <th>Id</th>
                            <th>Name</th>
                            <th className="text-end">Price</th>
                            <th className="text-end">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {products.length === 0 && (
                            <tr>
                                <td colSpan="4" className="text-muted">No products.</td>
                            </tr>
                        )}
                        {products.map((p) => (
                            <tr key={p.id}>
                                <td>{p.id}</td>
                                <td>{p.name}</td>
                                <td className="text-end">₹{p.price}</td>
                                <td className="text-end">
                                    <button  className="btn btn-sm btn-outline-primary me-2" onClick={() => startEdit(p)}>
                                        Edit
                                    </button>
                                    <button className="btn btn-sm btn-outline-danger" onClick={() => remove(p.id)}>
                                        Delete
                                    </button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}
        </div>
    );
}

createRoot(document.getElementById("crud-root")).render(<ProductCrud />);
