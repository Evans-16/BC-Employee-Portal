"use client";
import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import AuthLayout from "@/components/AuthLayout";
import { api } from "@/lib/api";
import { useAuth } from "@/context/AuthContext";

export default function LoginPage() {
  const router = useRouter();
  const { setEmployee } = useAuth();

  const [form, setForm]       = useState({ employeeNo: "", password: "" });
  const [error, setError]     = useState("");
  const [loading, setLoading] = useState(false);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm((f) => ({ ...f, [e.target.name]: e.target.value }));
    setError("");
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setLoading(true);
    try {
      const res = await api.login(form.employeeNo.trim(), form.password);
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
        setError(res.message || "Login failed. Please check your details.");
      }
    } catch {
      setError("Could not connect to the server. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthLayout
      title="Welcome back"
      subtitle="Sign in to your KNTC Employee Portal account"
      footer={
        <>
          Don&apos;t have an account?{" "}
          <Link href="/signup" className="text-brand font-semibold hover:underline">
            Register here
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
            className="input"
          />
        </div>

        <div>
          <label htmlFor="password" className="label">Password</label>
          <input
            id="password"
            name="password"
            type="password"
            autoComplete="current-password"
            placeholder="Enter your password"
            value={form.password}
            onChange={handleChange}
            required
            className="input"
          />
        </div>

        <button type="submit" disabled={loading} className="btn-primary mt-2">
          {loading ? "Signing in…" : "Sign in"}
        </button>

      </form>
    </AuthLayout>
  );
}