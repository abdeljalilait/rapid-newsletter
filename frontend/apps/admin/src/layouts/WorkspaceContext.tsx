import { WorkspaceDto } from "@rapid/api-client";
import { createContext, ReactNode, useContext } from "react";

const WorkspaceContext = createContext<WorkspaceDto | null>(null);

export function WorkspaceProvider({ workspace, children }: { workspace: WorkspaceDto; children: ReactNode }) {
  return <WorkspaceContext.Provider value={workspace}>{children}</WorkspaceContext.Provider>;
}

export function useWorkspace() {
  const workspace = useContext(WorkspaceContext);
  if (!workspace) throw new Error("Workspace context is missing");
  return workspace;
}
