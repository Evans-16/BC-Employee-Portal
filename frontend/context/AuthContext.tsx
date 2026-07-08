"use client";
import { createContext, useContext, useEffect, useState, ReactNode } from "react";

export interface Employee {
  employeeNo: string;
  firstName: string;
  lastName: string;
  email: string;
  jobTitle: string;
  phoneNo: string;
  gender: string;
  employmentType: string;
  status: string;
}

interface AuthCtx {
  employee: Employee | null;
  setEmployee: (e: Employee | null) => void;
  logout: () => void;
  isLoading: boolean;
}

const AuthContext = createContext<AuthCtx | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [employee, setEmployeeState] = useState<Employee | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Rehydrate session from localStorage on first load
  useEffect(() => {
    try {
      const stored = localStorage.getItem("employee");
      if (stored) setEmployeeState(JSON.parse(stored));
    } catch {
      localStorage.removeItem("employee");
    } finally {
      setIsLoading(false);
    }
  }, []);

  const setEmployee = (e: Employee | null) => {
    setEmployeeState(e);
    if (e) localStorage.setItem("employee", JSON.stringify(e));
    else localStorage.removeItem("employee");
  };

  const logout = () => {
    setEmployee(null);
    window.location.href = "/login";
  };

  return (
    <AuthContext.Provider value={{ employee, setEmployee, logout, isLoading }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}