import { LogoutOutlined } from "@ant-design/icons";
import { Button, Layout, Space, Typography } from "antd";
import { Outlet, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

const { Header, Content } = Layout;

export function AppLayout() {
  const { logout } = useAuth();
  const navigate = useNavigate();

  return (
    <Layout style={{ minHeight: "100vh" }}>
      <Header style={{ display: "flex", alignItems: "center", justifyContent: "space-between", paddingInline: 24 }}>
        <Typography.Text style={{ color: "#fff", fontSize: 18, fontWeight: 700 }}>
          Rapid Newsletter
        </Typography.Text>
        <Space>
          <Button onClick={() => navigate("/workspaces")}>Workspaces</Button>
          <Button icon={<LogoutOutlined />} onClick={() => logout().then(() => navigate("/login"))}>
            Logout
          </Button>
        </Space>
      </Header>
      <Outlet />
    </Layout>
  );
}

export { Content };
