"use client";
import { useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/context/AuthContext";
import { api } from "@/lib/api";

export default function EditProfilePage() {
  const { employee, setEmployee } = useAuth();
  const router = useRouter();

  const [form, setForm] = useState({
    firstName:    employee?.firstName    ?? "",
    lastName:     employee?.lastName     ?? "",
    companyEmail: employee?.email        ?? "",
    phoneNo:      employee?.phoneNo      ?? "",
    jobTitle:     employee?.jobTitle     ?? "",
  });
  const [error,   setError]   = useState("");
  const [success, setSuccess] = useState("");
  const [loading, setLoading] = useState(false);

  if (!employee) return null;

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm((f) => ({ ...f, [e.target.name]: e.target.value }));
    setError("");
    setSuccess("");
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setSuccess("");

    // Only send fields that actually changed
    const patch: Record<string, string> = {};
    if (form.firstName    !== employee.firstName)  patch.firstName    = form.firstName;
    if (form.lastName     !== employee.lastName)   patch.lastName     = form.lastName;
    if (form.companyEmail !== employee.email)      patch.companyEmail = form.companyEmail;
    if (form.phoneNo      !== employee.phoneNo)    patch.phoneNo      = form.phoneNo;
    if (form.jobTitle     !== employee.jobTitle)   patch.jobTitle     = form.jobTitle;

    if (Object.keys(patch).length === 0) {
      setError("No changes detected.");
      return;
    }

    setLoading(true);
    try {
      const res = await api.updateEmployee(employee.employeeNo, patch);
      if (res.success && res.data) {
        const d = res.data as any;
        // Update the stored session with fresh data from BC
        setEmployee({
          ...employee,
          firstName: d.firstName,
          lastName:  d.lastName,
          email:     d.email,
          phoneNo:   d.phoneNo,
          jobTitle:  d.jobTitle,
        });
        setSuccess("Profile updated successfully.");
      } else {
        setError(res.message || "Update failed. Please try again.");
      }
    } catch {
      setError("Could not connect to the server. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="max-w-xl space-y-6">

      {/* Header */}
      <div>
        <button
          onClick={() => router.back()}
          className="text-xs text-gray-500 hover:text-gray-700 font-medium mb-3 flex items-center gap-1"
        >
          ← Back
        </button>
        <h1 className="text-xl font-bold text-gray-900">Edit Profile</h1>
        <p className="text-sm text-gray-500 mt-1">
          Changes are saved directly to your Business Central record.
        </p>
      </div>

      <div className="card">
        <form onSubmit={handleSubmit} className="space-y-5">

          {error   && <div className="alert-error">{error}</div>}
          {success && <div className="alert-success">{success}</div>}

          <div className="grid sm:grid-cols-2 gap-4">
            <div>
              <label htmlFor="firstName" className="label">First Name</label>
              <input
                id="firstName" name="firstName" type="text"
                value={form.firstName} onChange={handleChange}
                className="input" required
              />
            </div>
            <div>
              <label htmlFor="lastName" className="label">Last Name</label>
              <input
                id="lastName" name="lastName" type="text"
                value={form.lastName} onChange={handleChange}
                className="input" required
              />
            </div>
          </div>

          <div>
            <label htmlFor="companyEmail" className="label">Company Email</label>
            <input
              id="companyEmail" name="companyEmail" type="email"
              value={form.companyEmail} onChange={handleChange}
              className="input"
            />
          </div>

          <div>
            <label htmlFor="phoneNo" className="label">Phone Number</label>
            <input
              id="phoneNo" name="phoneNo" type="tel"
              value={form.phoneNo} onChange={handleChange}
              placeholder="e.g. +254 700 000 000"
              className="input"
            />
          </div>

          <div>
            <label htmlFor="jobTitle" className="label">Job Title</label>
            <input
              id="jobTitle" name="jobTitle" type="text"
              value={form.jobTitle} onChange={handleChange}
              className="input"
            />
          </div>

          <div className="flex gap-3 pt-1">
            <button type="submit" disabled={loading}
              className="btn-primary" style={{ width: "auto", padding: "0.75rem 2rem" }}>
              {loading ? "Saving…" : "Save changes"}
            </button>
            <button type="button" onClick={() => router.back()} className="btn-ghost">
              Cancel
            </button>
          </div>

        </form>
      </div>
    </div>
  );
}