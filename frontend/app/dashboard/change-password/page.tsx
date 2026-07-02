"use client";
import { useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/context/AuthContext";
import { api } from "@/lib/api";

export default function ChangePasswordPage() {
  const { employee, logout } = useAuth();
  const router = useRouter();

  const [form, setForm] = useState({
    currentPassword: "",
    newPassword:     "",
    confirmPassword: "",
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

    if (form.newPassword.length < 8) {
      setError("New password must be at least 8 characters.");
      return;
    }
    if (form.newPassword !== form.confirmPassword) {
      setError("New passwords do not match.");
      return;
    }
    if (form.currentPassword === form.newPassword) {
      setError("New password must be different from your current password.");
      return;
    }

    setLoading(true);
    try {
      const res = await api.changePassword(
        employee.employeeNo,
        form.currentPassword,
        form.newPassword
      );
      if (res.success) {
        setSuccess("Password changed successfully. You will be signed out in 3 seconds.");
        setForm({ currentPassword: "", newPassword: "", confirmPassword: "" });
        // Sign the user out after a short delay so they log back in with the new password
        setTimeout(() => logout(), 3000);
      } else {
        setError(res.message || "Failed to change password. Please try again.");
      }
    } catch {
      setError("Could not connect to the server. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  // Password strength indicator
  const strength = (() => {
    const p = form.newPassword;
    if (!p) return null;
    let score = 0;
    if (p.length >= 8)  score++;
    if (p.length >= 12) score++;
    if (/[A-Z]/.test(p)) score++;
    if (/[0-9]/.test(p)) score++;
    if (/[^a-zA-Z0-9]/.test(p)) score++;
    if (score <= 1) return { label: "Weak",   color: "bg-red-400",    width: "w-1/4" };
    if (score <= 3) return { label: "Fair",   color: "bg-yellow-400", width: "w-2/4" };
    if (score <= 4) return { label: "Good",   color: "bg-blue-400",   width: "w-3/4" };
    return               { label: "Strong", color: "bg-green-500",  width: "w-full" };
  })();

  return (
    <div className="max-w-md space-y-6">

      {/* Header */}
      <div>
        <button
          onClick={() => router.back()}
          className="text-xs text-gray-500 hover:text-gray-700 font-medium mb-3 flex items-center gap-1"
        >
          ← Back
        </button>
        <h1 className="text-xl font-bold text-gray-900">Change Password</h1>
        <p className="text-sm text-gray-500 mt-1">
          You&apos;ll be signed out after changing your password.
        </p>
      </div>

      <div className="card">
        <form onSubmit={handleSubmit} className="space-y-5">

          {error   && <div className="alert-error">{error}</div>}
          {success && <div className="alert-success">{success}</div>}

          <div>
            <label htmlFor="currentPassword" className="label">Current Password</label>
            <input
              id="currentPassword" name="currentPassword" type="password"
              autoComplete="current-password"
              placeholder="Enter your current password"
              value={form.currentPassword} onChange={handleChange}
              required className="input"
            />
          </div>

          <div>
            <label htmlFor="newPassword" className="label">New Password</label>
            <input
              id="newPassword" name="newPassword" type="password"
              autoComplete="new-password"
              placeholder="Minimum 8 characters"
              value={form.newPassword} onChange={handleChange}
              required className="input"
            />
            {/* Strength bar */}
            {strength && (
              <div className="mt-2">
                <div className="h-1 bg-gray-200 rounded-full overflow-hidden">
                  <div className={`h-full rounded-full transition-all duration-300
                                  ${strength.color} ${strength.width}`} />
                </div>
                <p className="text-xs text-gray-500 mt-1">{strength.label} password</p>
              </div>
            )}
          </div>

          <div>
            <label htmlFor="confirmPassword" className="label">Confirm New Password</label>
            <input
              id="confirmPassword" name="confirmPassword" type="password"
              autoComplete="new-password"
              placeholder="Re-enter your new password"
              value={form.confirmPassword} onChange={handleChange}
              required
              className={`input ${
                form.confirmPassword && form.confirmPassword !== form.newPassword
                  ? "input-error" : ""
              }`}
            />
            {form.confirmPassword && form.confirmPassword !== form.newPassword && (
              <p className="text-red-500 text-xs mt-1.5">Passwords do not match.</p>
            )}
          </div>

          <div className="flex gap-3 pt-1">
            <button type="submit" disabled={loading || !!success}
              className="btn-primary" style={{ width: "auto", padding: "0.75rem 2rem" }}>
              {loading ? "Saving…" : "Change password"}
            </button>
            <button type="button" onClick={() => router.back()} className="btn-ghost">
              Cancel
            </button>
          </div>

        </form>
      </div>

      {/* Security tip */}
      <div className="card bg-brand-subtle border-brand-subtle">
        <p className="text-xs text-navy font-semibold mb-1">💡 Password tips</p>
        <ul className="text-xs text-gray-600 space-y-0.5 list-disc list-inside">
          <li>Use at least 12 characters</li>
          <li>Mix uppercase, lowercase, numbers and symbols</li>
          <li>Don&apos;t reuse passwords from other accounts</li>
        </ul>
      </div>

    </div>
  );
}