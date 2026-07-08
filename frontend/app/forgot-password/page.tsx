"use client";
import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import AuthLayout from "@/components/AuthLayout";
import { api } from "@/lib/api";

export default function ForgotPasswordPage() {
  const router = useRouter();

  const [form, setForm] = useState({
    employeeNo:      "",
    nationalId:      "",
    newPassword:     "",
    confirmPassword: "",
  });
  const [error,   setError]   = useState("");
  const [success, setSuccess] = useState("");
  const [loading, setLoading] = useState(false);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm((f) => ({ ...f, [e.target.name]: e.target.value }));
    setError("");
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
      setError("Passwords do not match.");
      return;
    }

    setLoading(true);
    try {
      const res = await api.forgotPassword(
        form.employeeNo.trim(),
        form.nationalId.trim(),
        form.newPassword
      );
      if (res.success) {
        setSuccess("Password reset successfully. Redirecting you to sign in…");
        setTimeout(() => router.push("/login"), 2500);
      } else {
        setError(res.message || "Could not reset your password. Please check your details.");
      }
    } catch {
      setError("Could not connect to the server. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthLayout
      title="Reset your password"
      subtitle="Verify your identity to set a new password"
      footer={
        <>
          Remembered your password?{" "}
          <Link href="/login" className="text-brand font-semibold hover:underline">
            Back to sign in
          </Link>
        </>
      }
    >
      <form onSubmit={handleSubmit} className="space-y-5">

        {error   && <div className="alert-error">{error}</div>}
        {success && <div className="alert-success">{success}</div>}

        <div>
          <label htmlFor="employeeNo" className="label">Employee Number</label>
          <input
            id="employeeNo"
            name="employeeNo"
            type="text"
            autoComplete="username"
            placeholder="e.g. EMP-001"
            value={form.employeeNo}
            onChange={handleChange}
            required
            disabled={!!success}
            className="input"
          />
        </div>

        <div>
          <label htmlFor="nationalId" className="label">National ID Number</label>
          <input
            id="nationalId"
            name="nationalId"
            type="text"
            placeholder="As it appears on your HR record"
            value={form.nationalId}
            onChange={handleChange}
            required
            disabled={!!success}
            className="input"
          />
          <p className="text-xs text-gray-400 mt-1.5">
            We use this to confirm it's really you, since you don't have your password.
          </p>
        </div>

        <div>
          <label htmlFor="newPassword" className="label">New Password</label>
          <input
            id="newPassword"
            name="newPassword"
            type="password"
            autoComplete="new-password"
            placeholder="Minimum 8 characters"
            value={form.newPassword}
            onChange={handleChange}
            required
            disabled={!!success}
            className="input"
          />
        </div>

        <div>
          <label htmlFor="confirmPassword" className="label">Confirm New Password</label>
          <input
            id="confirmPassword"
            name="confirmPassword"
            type="password"
            autoComplete="new-password"
            placeholder="Re-enter your new password"
            value={form.confirmPassword}
            onChange={handleChange}
            required
            disabled={!!success}
            className={`input ${
              form.confirmPassword && form.confirmPassword !== form.newPassword
                ? "input-error" : ""
            }`}
          />
          {form.confirmPassword && form.confirmPassword !== form.newPassword && (
            <p className="text-red-500 text-xs mt-1.5">Passwords do not match.</p>
          )}
        </div>

        <button type="submit" disabled={loading || !!success} className="btn-primary mt-2">
          {loading ? "Resetting…" : "Reset password"}
        </button>

      </form>
    </AuthLayout>
  );
}