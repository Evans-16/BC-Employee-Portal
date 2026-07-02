"use client";
import { useEffect } from "react";
import { useRouter, usePathname } from "next/navigation";
import Link from "next/link";
import { useAuth } from "@/context/AuthContext";
import { api } from "@/lib/api";

const NAV = [
  { href: "/dashboard",                label: "Dashboard",       icon: "⊞" },
  { href: "/dashboard/edit-profile",   label: "Edit Profile",    icon: "✏️" },
  { href: "/dashboard/change-password",label: "Change Password", icon: "🔐" },
];

export default function DashboardLayout({ children }: { children: React.ReactNode }) {
  const { employee, logout, isLoading } = useAuth();
  const router   = useRouter();
  const pathname = usePathname();

  // Redirect to login if no session
  useEffect(() => {
    if (!isLoading && !employee) router.replace("/login");
  }, [employee, isLoading, router]);

  if (isLoading || !employee) {
    return (
      <div className="min-h-screen bg-brand-subtle flex items-center justify-center">
        <div className="text-navy font-medium text-sm animate-pulse">Loading…</div>
      </div>
    );
  }

  const handleLogout = async () => {
    await api.logout();
    logout();
  };

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col">

      {/* ── Top navbar ──────────────────────────────────────────── */}
      <header className="bg-navy border-b border-navy-dark sticky top-0 z-20">
        <div className="max-w-6xl mx-auto px-4 sm:px-6 h-14 flex items-center justify-between">

          <div className="flex items-center gap-3">
            <div className="w-8 h-8 rounded-lg bg-white/15 flex items-center justify-center">
              <span className="text-white font-bold text-xs">C</span>
            </div>
            <span className="text-white font-semibold text-sm hidden sm:block">
              CRONUS International Ltd. Employee Portal
            </span>
          </div>

          <div className="flex items-center gap-4">
            <span className="text-blue-200 text-xs hidden sm:block">
              {employee.firstName} {employee.lastName}
            </span>
            <button
              onClick={handleLogout}
              className="text-white/70 hover:text-white text-xs font-medium
                         border border-white/20 hover:border-white/40
                         px-3 py-1.5 rounded-md transition-colors duration-150"
            >
              Sign out
            </button>
          </div>
        </div>
      </header>

      <div className="flex-1 flex max-w-6xl mx-auto w-full px-4 sm:px-6 py-8 gap-8">

        {/* ── Sidebar ─────────────────────────────────────────────── */}
        <aside className="hidden md:flex flex-col w-52 shrink-0">
          <nav className="bg-white rounded-xl border border-gray-200 shadow-sm
                          overflow-hidden sticky top-22">
            {NAV.map(({ href, label, icon }) => {
              const active = pathname === href;
              return (
                <Link
                  key={href}
                  href={href}
                  className={`flex items-center gap-3 px-4 py-3.5 text-sm font-medium
                              border-b border-gray-100 last:border-0 transition-colors duration-100
                              ${active
                                ? "bg-brand-subtle text-navy border-l-4 border-l-brand pl-3"
                                : "text-gray-600 hover:bg-gray-50 hover:text-gray-900"
                              }`}
                >
                  <span className="text-base">{icon}</span>
                  {label}
                </Link>
              );
            })}

            <button
              onClick={handleLogout}
              className="flex items-center gap-3 px-4 py-3.5 text-sm font-medium
                         text-red-500 hover:bg-red-50 w-full text-left
                         transition-colors duration-100"
            >
              <span className="text-base">🚪</span>
              Sign out
            </button>
          </nav>
        </aside>

        {/* ── Main content ─────────────────────────────────────────── */}
        <main className="flex-1 min-w-0">{children}</main>
      </div>

      {/* ── Mobile bottom nav ──────────────────────────────────────── */}
      <nav className="md:hidden fixed bottom-0 inset-x-0 bg-white border-t
                      border-gray-200 flex z-20">
        {NAV.map(({ href, label, icon }) => {
          const active = pathname === href;
          return (
            <Link
              key={href}
              href={href}
              className={`flex-1 flex flex-col items-center justify-center py-2.5 gap-0.5
                          text-[10px] font-medium transition-colors
                          ${active ? "text-brand" : "text-gray-400"}`}
            >
              <span className="text-lg leading-none">{icon}</span>
              {label}
            </Link>
          );
        })}
      </nav>

      {/* Bottom padding for mobile nav */}
      <div className="h-16 md:hidden" />
    </div>
  );
}