import { Card, Col, Row, Statistic, Typography } from "antd";
import { useQuery } from "@tanstack/react-query";
import { analyticsApi } from "@rapid/api-client";
import { useAuth } from "../../auth/AuthContext";
import { useWorkspace } from "../../layouts/WorkspaceContext";

export function DashboardPage() {
  const { client } = useAuth();
  const workspace = useWorkspace();
  const query = useQuery({
    queryKey: ["analytics", workspace.id],
    queryFn: () => analyticsApi(client, workspace.id).overview()
  });

  const stats = query.data ?? {
    totalSubscribers: 0,
    freeSubscribers: 0,
    paidSubscribers: 0,
    activeSubscriptions: 0,
    emailsSentThisMonth: 0
  };

  return (
    <>
      <Typography.Title level={2}>{workspace.name}</Typography.Title>
      <Row gutter={[16, 16]}>
        <Metric title="Total subscribers" value={stats.totalSubscribers} />
        <Metric title="Free subscribers" value={stats.freeSubscribers} />
        <Metric title="Paid subscribers" value={stats.paidSubscribers} />
        <Metric title="Active subscriptions" value={stats.activeSubscriptions} />
        <Metric title="Emails this month" value={stats.emailsSentThisMonth} />
      </Row>
    </>
  );
}

function Metric({ title, value }: { title: string; value: number }) {
  return (
    <Col xs={24} sm={12} lg={8} xl={6}>
      <Card>
        <Statistic title={title} value={value} />
      </Card>
    </Col>
  );
}
