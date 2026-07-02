"use client";
import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import AuthLayout from "@/components/AuthLayout";
import { api } from "@/lib/api";
import { useAuth } from "@/context/AuthContext";

export default function SignupPage() {
  const router = useRouter();
  const { setEmployee } = useAuth();

  const [form, setForm] = useState({ employeeNo: "", password: "", confirm: "" });
  const [error, setError]     = useState("");
  const [loading, setLoading] = useState(false);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm((f) => ({ ...f, [e.target.name]: e.target.value }));
    setError("");
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");

    if (form.password !== form.confirm) {
      setError("Passwords do not match.");
      return;
    }
    if (form.password.length < 8) {
      setError("Password must be at least 8 characters.");
      return;
    }

    setLoading(true);
    try {
      const res = await api.signup(form.employeeNo.trim(), form.password);
      if (res.success && res.data) {
        const d = res.data as any;
        setEmployee({
          employeeNo: d.employeeNo,
          firstName:  d.firstName,
          lastName:   d.lastName,
          email:      d.email,
          jobTitle:   d.jobTitle,
          phoneNo:    d.phoneNo,
        });
        router.push("/dashboard");
      } else {
        setError(res.message || "Signup failed. Please try again.");
      }
    } catch {
      setError("Could not connect to the server. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthLayout
      title="Create your account"
      subtitle="Register with your Employee Number to get started"
      footer={
        <>
          Already registered?{" "}
          <Link href="/login" className="text-brand font-semibold hover:underline">
            Sign in
          </Link>
        </>
      }
    >
      <form onSubmit={handleSubmit} className="space-y-5">

        {error && <div className="alert-error">{error}</div>}

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
            className={`input ${error && !form.employeeNo ? "input-error" : ""}`}
          />
        </div>

        <div>
          <label htmlFor="password" className="label">Password</label>
          <input
            id="password"
            name="password"
            type="password"
            autoComplete="new-password"
            placeholder="Minimum 8 characters"
            value={form.password}
            onChange={handleChange}
            required
            className="input"
          />
        </div>

        <div>
          <label htmlFor="confirm" className="label">Confirm Password</label>
          <input
            id="confirm"
            name="confirm"
            type="password"
            autoComplete="new-password"
            placeholder="Re-enter your password"
            value={form.confirm}
            onChange={handleChange}
            required
            className={`input ${
              form.confirm && form.confirm !== form.password ? "input-error" : ""
            }`}
          />
          {form.confirm && form.confirm !== form.password && (
            <p className="text-red-500 text-xs mt-1.5">Passwords do not match.</p>
          )}
        </div>

        <button type="submit" disabled={loading} className="btn-primary mt-2">
          {loading ? "Creating account…" : "Create account"}
        </button>

      </form>
    </AuthLayout>
  );
}