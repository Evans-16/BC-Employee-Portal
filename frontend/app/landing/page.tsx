import Link from "next/link";

export default function LandingPage() {
  return (
    <div className="min-h-screen flex flex-col">

      {/* ── Nav ─────────────────────────────────────────────────── */}
      <header className="absolute top-0 inset-x-0 z-10">
        <div className="max-w-6xl mx-auto px-6 py-5 flex items-center justify-between">
          <div className="flex items-center gap-3">
            {/* Logo mark */}
            <div className="w-9 h-9 rounded-lg bg-white/20 backdrop-blur-sm
                            flex items-center justify-center border border-white/30">
              <span className="text-white font-bold text-sm tracking-tight">C</span>
            </div>
            <span className="text-white font-semibold text-sm tracking-wide">
              CRONUS International Ltd. Employee Portal
            </span>
          </div>
          <div className="flex items-center gap-3">
            <Link
              href="/login"
              className="text-white/80 hover:text-white text-sm font-medium
                         transition-colors duration-150"
            >
              Sign in
            </Link>
            <Link
              href="/signup"
              className="bg-white text-navy font-semibold text-sm px-4 py-2 rounded-lg
                         hover:bg-brand-subtle transition-colors duration-150"
            >
              Register
            </Link>
          </div>
        </div>
      </header>

      {/* ── Hero ─────────────────────────────────────────────────── */}
      {/* Signature element: diagonal split — navy left / brand-blue right */}
      <main className="relative flex-1 flex items-center overflow-hidden"
            style={{ minHeight: "100vh" }}>

        {/* Gradient background */}
        <div
          className="absolute inset-0"
          style={{
            background:
              "linear-gradient(135deg, #122647 0%, #1B3A6B 45%, #2563EB 100%)",
          }}
        />

        {/* Subtle grid overlay for depth */}
        <div
          className="absolute inset-0 opacity-[0.04]"
          style={{
            backgroundImage:
              "repeating-linear-gradient(0deg,#fff 0,#fff 1px,transparent 1px,transparent 60px)," +
              "repeating-linear-gradient(90deg,#fff 0,#fff 1px,transparent 1px,transparent 60px)",
          }}
        />

        <div className="relative max-w-6xl mx-auto px-6 py-32 grid md:grid-cols-2
                        gap-16 items-center w-full">

          {/* Left — copy */}
          <div>
            <p className="text-blue-300 text-xs font-semibold uppercase tracking-[0.2em] mb-5">
              CRONUS International Ltd.
            </p>
            <h1 className="text-5xl md:text-6xl font-bold text-white leading-tight
                           tracking-tight mb-6">
              Your work,<br />
              <span className="text-blue-300">at your</span><br />
              fingertips.
            </h1>
            <p className="text-blue-100 text-lg leading-relaxed mb-10 max-w-md">
              Access your profile, update your details, and manage your account —
              all in one secure place built for KNTC staff.
            </p>
            <div className="flex flex-col sm:flex-row gap-3">
              <Link
                href="/signup"
                className="inline-flex items-center justify-center bg-white text-navy
                           font-semibold px-8 py-3.5 rounded-lg text-sm
                           hover:bg-brand-subtle transition-colors duration-150"
              >
                Create your account
              </Link>
              <Link
                href="/login"
                className="inline-flex items-center justify-center border border-white/40
                           text-white font-semibold px-8 py-3.5 rounded-lg text-sm
                           hover:bg-white/10 transition-colors duration-150"
              >
                Sign in
              </Link>
            </div>
          </div>

          {/* Right — feature tiles */}
          <div className="hidden md:grid grid-cols-2 gap-4">
            {[
              { icon: "👤", title: "Your Profile",    body: "View your name, job title, contact details and employment info." },
              { icon: "✏️", title: "Edit Details",    body: "Keep your profile up to date directly from the portal." },
              { icon: "🔐", title: "Secure Access",   body: "Your account is protected with industry-standard encryption." },
              { icon: "📋", title: "Self-Service",    body: "No more back-and-forth with HR for basic profile changes." },
            ].map((f) => (
              <div
                key={f.title}
                className="bg-white/10 backdrop-blur-sm border border-white/20
                           rounded-xl p-5 hover:bg-white/15 transition-colors duration-200"
              >
                <div className="text-2xl mb-3">{f.icon}</div>
                <h3 className="text-white font-semibold text-sm mb-1.5">{f.title}</h3>
                <p className="text-blue-200 text-xs leading-relaxed">{f.body}</p>
              </div>
            ))}
          </div>
        </div>
      </main>

      {/* ── Footer ───────────────────────────────────────────────── */}
      <footer className="bg-navy-dark py-5">
        <p className="text-center text-blue-300/60 text-xs tracking-wide">
          © {new Date().getFullYear()} CRONUS International Ltd. · Internal Use Only
        </p>
      </footer>
    </div>
  );
}