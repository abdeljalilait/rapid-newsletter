import { Card, Typography } from "antd";
import { Outlet } from "react-router-dom";

export function AuthLayout() {
  return (
    <main style={{ minHeight: "100vh", display: "grid", placeItems: "center", padding: 24 }}>
      <Card style={{ width: "100%", maxWidth: 420 }}>
        <Typography.Title level={2} style={{ marginTop: 0 }}>
          Rapid Newsletter
        </Typography.Title>
        <Outlet />
      </Card>
    </main>
  );
}
