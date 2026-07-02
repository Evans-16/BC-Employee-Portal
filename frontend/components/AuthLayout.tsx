import Link from "next/link";
import { ReactNode } from "react";

interface Props {
  title: string;
  subtitle: string;
  children: ReactNode;
  footer: ReactNode;
}

export default function AuthLayout({ title, subtitle, children, footer }: Props) {
  return (
    <div className="min-h-screen bg-brand-subtle flex flex-col">

      {/* Top navy strip */}
      <div className="h-1.5 bg-gradient-to-r from-navy via-brand to-blue-400" />

      <div className="flex-1 flex flex-col items-center justify-center px-4 py-12">

        {/* Logo */}
        <Link href="/landing" className="flex items-center gap-2.5 mb-8 group">
          <div className="w-10 h-10 rounded-xl bg-navy flex items-center justify-center
                          group-hover:bg-navy-light transition-colors duration-150">
            <span className="text-white font-bold text-base tracking-tight">C</span>
          </div>
          <div>
            <p className="text-navy font-bold text-sm leading-tight">CRONUS International Ltd.</p>
            <p className="text-gray-500 text-[10px] uppercase tracking-widest leading-tight">
              Employee Portal
            </p>
          </div>
        </Link>

        {/* Card */}
        <div className="w-full max-w-md bg-white rounded-2xl shadow-lg
                        border border-gray-200 overflow-hidden">

          {/* Card header */}
          <div className="bg-navy px-8 py-6">
            <h1 className="text-white font-bold text-xl tracking-tight">{title}</h1>
            <p className="text-blue-200 text-sm mt-1">{subtitle}</p>
          </div>

          {/* Card body */}
          <div className="px-8 py-7">{children}</div>
        </div>

        {/* Footer link */}
        <div className="mt-5 text-sm text-gray-600">{footer}</div>
      </div>

      <footer className="py-5 text-center text-gray-400 text-xs">
        © {new Date().getFullYear()} CRONUS International Ltd.
      </footer>
    </div>
  );
}