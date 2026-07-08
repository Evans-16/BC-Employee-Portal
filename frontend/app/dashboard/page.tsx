"use client";
import Link from "next/link";
import { useAuth } from "@/context/AuthContext";

function InfoRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex flex-col sm:flex-row sm:items-center py-3.5
                    border-b border-gray-100 last:border-0">
      <span className="text-xs font-semibold text-gray-400 uppercase tracking-wider
                       sm:w-40 shrink-0 mb-0.5 sm:mb-0">
        {label}
      </span>
      <span className="text-sm text-gray-800 font-medium">
        {value || <span className="text-gray-400 font-normal italic">Not set</span>}
      </span>
    </div>
  );
}

function StatusBadge({ status }: { status: string }) {
  const s = (status || "").toLowerCase();
  const config =
    s === "active"
      ? { dot: "bg-green-400",  text: "text-green-50",  bg: "bg-green-500/20",  border: "border-green-300/40" }
      : s === "inactive"
      ? { dot: "bg-amber-400",  text: "text-amber-50",  bg: "bg-amber-500/20",  border: "border-amber-300/40" }
      : s === "terminated"
      ? { dot: "bg-red-400",    text: "text-red-50",    bg: "bg-red-500/20",    border: "border-red-300/40" }
      : { dot: "bg-gray-300",   text: "text-gray-50",   bg: "bg-white/10",      border: "border-white/20" };

  return (
    <span
      className={`inline-flex items-center gap-1.5 ${config.bg} ${config.border} border
                  rounded-full px-3 py-1 text-xs font-semibold ${config.text}`}
    >
      <span className={`w-1.5 h-1.5 rounded-full ${config.dot}`} />
      {status || "Unknown"}
    </span>
  );
}

export default function DashboardPage() {
  const { employee } = useAuth();
  if (!employee) return null;

  const initials =
    `${employee.firstName?.[0] ?? ""}${employee.lastName?.[0] ?? ""}`.toUpperCase() || "?";

  const greeting = (() => {
    const h = new Date().getHours();
    if (h < 12) return "Good morning";
    if (h < 17) return "Good afternoon";
    return "Good evening";
  })();

  return (
    <div className="space-y-6">

      {/* ── Welcome banner ─────────────────────────────────────────── */}
      <div
        className="rounded-xl p-6 sm:p-8 text-white"
        style={{
          background: "linear-gradient(135deg, #1B3A6B 0%, #2563EB 100%)",
        }}
      >
        <div className="flex items-start justify-between gap-4 mb-2">
          <div className="flex items-center gap-4">
            {/* Avatar — back to initials only */}
            <div className="w-14 h-14 rounded-full bg-white/20 border-2 border-white/30
                            flex items-center justify-center shrink-0">
              <span className="font-bold text-lg text-white">{initials}</span>
            </div>
            <div>
              <p className="text-blue-200 text-xs font-medium uppercase tracking-widest mb-0.5">
                {greeting}
              </p>
              <h1 className="text-xl sm:text-2xl font-bold tracking-tight">
                {employee.firstName} {employee.lastName}
              </h1>
              <p className="text-blue-200 text-sm mt-0.5">{employee.jobTitle || "Cronus International Ltd. Staff"}</p>
            </div>
          </div>

          {/* Active / inactive status */}
          <StatusBadge status={employee.status} />
        </div>
      </div>

      <div className="grid md:grid-cols-3 gap-6">

        {/* ── Profile card ────────────────────────────────────────── */}
        <div className="md:col-span-2 card">
          <div className="flex items-center justify-between mb-4">
            <h2 className="font-semibold text-gray-900">Profile Details</h2>
            <Link
              href="/dashboard/edit-profile"
              className="text-xs text-brand font-semibold hover:underline"
            >
              Edit →
            </Link>
          </div>

          <InfoRow label="Employee No."     value={employee.employeeNo} />
          <InfoRow label="First Name"       value={employee.firstName} />
          <InfoRow label="Last Name"        value={employee.lastName} />
          <InfoRow label="Gender"           value={employee.gender} />
          <InfoRow label="Job Title"        value={employee.jobTitle} />
          <InfoRow label="Employment Type"  value={employee.employmentType} />
          <InfoRow label="Email"            value={employee.email} />
          <InfoRow label="Phone"            value={employee.phoneNo} />
        </div>

        {/* ── Quick actions ──────────────────────────────────────── */}
        <div className="space-y-4">
          <h2 className="font-semibold text-gray-900">Quick Actions</h2>

          <Link
            href="/dashboard/edit-profile"
            className="card-accent flex items-start gap-4 hover:shadow-md
                       transition-shadow duration-150 group block"
          >
            <div className="w-10 h-10 rounded-lg bg-brand-subtle flex items-center
                            justify-center shrink-0">
              <span className="text-lg">✏️</span>
            </div>
            <div>
              <p className="text-sm font-semibold text-gray-900
                            group-hover:text-brand transition-colors">
                Edit Profile
              </p>
              <p className="text-xs text-gray-500 mt-0.5 leading-relaxed">
                Update your name, email, phone or job title.
              </p>
            </div>
          </Link>

          <Link
            href="/dashboard/change-password"
            className="card-accent flex items-start gap-4 hover:shadow-md
                       transition-shadow duration-150 group block"
          >
            <div className="w-10 h-10 rounded-lg bg-brand-subtle flex items-center
                            justify-center shrink-0">
              <span className="text-lg">🔐</span>
            </div>
            <div>
              <p className="text-sm font-semibold text-gray-900
                            group-hover:text-brand transition-colors">
                Change Password
              </p>
              <p className="text-xs text-gray-500 mt-0.5 leading-relaxed">
                Update your portal login password.
              </p>
            </div>
          </Link>

          {/* Employee No. chip */}
          <div className="card bg-gray-50 border-gray-100">
            <p className="text-xs text-gray-500 uppercase tracking-wider font-semibold mb-1">
              Employee Number
            </p>
            <p className="text-navy font-bold text-lg">{employee.employeeNo}</p>
          </div>
        </div>
      </div>
    </div>
  );
}