import { Layout, Spin, Typography } from "antd";
import { Outlet, useParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { workspacesApi } from "@rapid/api-client";
import { useAuth } from "../auth/AuthContext";
import { Sidebar } from "../components/Sidebar";
import { WorkspaceProvider } from "./WorkspaceContext";

const { Sider, Content } = Layout;

export function WorkspaceLayout() {
  const { workspaceId = "" } = useParams();
  const { client } = useAuth();
  const api = workspacesApi(client);
  const workspaceQuery = useQuery({
    queryKey: ["workspace", workspaceId],
    queryFn: () => api.get(workspaceId),
    enabled: Boolean(workspaceId)
  });

  if (workspaceQuery.isLoading) return <Spin fullscreen />;
  if (!workspaceQuery.data) return <Typography.Text>Workspace not found.</Typography.Text>;

  return (
    <WorkspaceProvider workspace={workspaceQuery.data}>
      <Layout>
        <Sider breakpoint="lg" collapsedWidth="0">
          <Sidebar />
        </Sider>
        <Content style={{ padding: 24, minHeight: "calc(100vh - 64px)" }}>
          <Outlet />
        </Content>
      </Layout>
    </WorkspaceProvider>
  );
}
