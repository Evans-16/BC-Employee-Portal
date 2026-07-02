const BASE = "http://localhost:5131";

async function request<T>(
  path: string,
  options: RequestInit = {}
): Promise<{ success: boolean; message: string; data?: T }> {
  const res = await fetch(`${BASE}${path}`, {
    headers: { "Content-Type": "application/json" },
    ...options,
  });
  const json = await res.json();
  return json;
}

export const api = {
  signup: (employeeNo: string, password: string) =>
    request("/api/auth/signup", {
      method: "POST",
      body: JSON.stringify({ employeeNo, password }),
    }),

  login: (employeeNo: string, password: string) =>
    request("/api/auth/login", {
      method: "POST",
      body: JSON.stringify({ employeeNo, password }),
    }),

  logout: () =>
    request("/api/auth/logout", { method: "POST" }),

  changePassword: (
    employeeNo: string,
    currentPassword: string,
    newPassword: string
  ) =>
    request("/api/auth/change-password", {
      method: "PUT",
      body: JSON.stringify({ employeeNo, currentPassword, newPassword }),
    }),

  getEmployee: (employeeNo: string) =>
    request(`/api/employees/${employeeNo}`),

  updateEmployee: (
    employeeNo: string,
    fields: {
      firstName?: string;
      lastName?: string;
      companyEmail?: string;
      phoneNo?: string;
      jobTitle?: string;
    }
  ) =>
    request(`/api/employees/${employeeNo}`, {
      method: "PUT",
      body: JSON.stringify(fields),
    }),
};