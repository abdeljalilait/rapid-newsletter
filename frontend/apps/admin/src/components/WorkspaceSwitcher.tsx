import { Select } from "antd";
import { WorkspaceDto } from "@rapid/api-client";
import { useNavigate } from "react-router-dom";

export function WorkspaceSwitcher({ currentId, workspaces }: { currentId: string; workspaces: WorkspaceDto[] }) {
  const navigate = useNavigate();

  return (
    <Select
      value={currentId}
      style={{ minWidth: 220 }}
      options={workspaces.map((workspace) => ({ value: workspace.id, label: workspace.name }))}
      onChange={(id) => navigate(`/${id}/dashboard`)}
    />
  );
}
