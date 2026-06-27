import {
  BarChartOutlined,
  CreditCardOutlined,
  MailOutlined,
  ProfileOutlined,
  SendOutlined,
  SettingOutlined,
  TagsOutlined,
  TeamOutlined,
  UnorderedListOutlined
} from "@ant-design/icons";
import { Menu } from "antd";
import { useMemo } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { useWorkspace } from "../layouts/WorkspaceContext";

export function Sidebar() {
  const workspace = useWorkspace();
  const navigate = useNavigate();
  const location = useLocation();

  const items = useMemo(() => [
    { key: "dashboard", icon: <BarChartOutlined />, label: "Dashboard" },
    { key: "subscribers", icon: <TeamOutlined />, label: "Subscribers" },
    { key: "posts", icon: <ProfileOutlined />, label: "Posts" },
    { key: "campaigns", icon: <SendOutlined />, label: "Campaigns" },
    { key: "tags", icon: <TagsOutlined />, label: "Tags" },
    { key: "lists", icon: <UnorderedListOutlined />, label: "Lists" },
    { key: "plans", icon: <CreditCardOutlined />, label: "Plans" },
    { key: "providers", icon: <MailOutlined />, label: "Providers" },
    { key: "payments", icon: <CreditCardOutlined />, label: "Payments" },
    { key: "settings", icon: <SettingOutlined />, label: "Settings" }
  ], []);

  const selected = items.find((item) => location.pathname.endsWith(`/${item.key}`))?.key ?? "dashboard";

  return (
    <Menu
      theme="dark"
      mode="inline"
      selectedKeys={[selected]}
      items={items}
      onClick={({ key }) => navigate(`/${workspace.id}/${key}`)}
    />
  );
}
