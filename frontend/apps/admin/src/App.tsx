import { Navigate, Route, Routes } from "react-router-dom";
import { ProtectedRoute } from "./auth/ProtectedRoute";
import { AppLayout } from "./layouts/AppLayout";
import { AuthLayout } from "./layouts/AuthLayout";
import { WorkspaceLayout } from "./layouts/WorkspaceLayout";
import { LoginPage } from "./pages/auth/LoginPage";
import { RegisterPage } from "./pages/auth/RegisterPage";
import { CampaignListPage } from "./pages/campaigns/CampaignListPage";
import { DashboardPage } from "./pages/dashboard/DashboardPage";
import { ListListPage } from "./pages/lists/ListListPage";
import { PaymentConfigPage } from "./pages/payments/PaymentConfigPage";
import { PlanListPage } from "./pages/plans/PlanListPage";
import { PostListPage } from "./pages/posts/PostListPage";
import { ProviderListPage } from "./pages/providers/ProviderListPage";
import { WorkspaceSettingsPage } from "./pages/settings/WorkspaceSettingsPage";
import { SubscriberListPage } from "./pages/subscribers/SubscriberListPage";
import { TagListPage } from "./pages/tags/TagListPage";
import { WorkspaceListPage } from "./pages/workspaces/WorkspaceListPage";

export default function App() {
  return (
    <Routes>
      <Route element={<AuthLayout />}>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
      </Route>
      <Route element={<ProtectedRoute />}>
        <Route element={<AppLayout />}>
          <Route path="/workspaces" element={<WorkspaceListPage />} />
          <Route path="/:workspaceId" element={<WorkspaceLayout />}>
            <Route index element={<Navigate to="dashboard" replace />} />
            <Route path="dashboard" element={<DashboardPage />} />
            <Route path="subscribers" element={<SubscriberListPage />} />
            <Route path="posts" element={<PostListPage />} />
            <Route path="campaigns" element={<CampaignListPage />} />
            <Route path="tags" element={<TagListPage />} />
            <Route path="lists" element={<ListListPage />} />
            <Route path="plans" element={<PlanListPage />} />
            <Route path="providers" element={<ProviderListPage />} />
            <Route path="payments" element={<PaymentConfigPage />} />
            <Route path="settings" element={<WorkspaceSettingsPage />} />
          </Route>
        </Route>
      </Route>
      <Route path="*" element={<Navigate to="/workspaces" replace />} />
    </Routes>
  );
}
