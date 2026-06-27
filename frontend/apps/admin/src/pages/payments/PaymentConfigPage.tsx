import { Button, Card, Form, Input, Select, Typography, message } from "antd";
import { useMutation, useQuery } from "@tanstack/react-query";
import { PaymentEnvironment, paymentsApi } from "@rapid/api-client";
import { queryClient } from "../../api/queryClient";
import { useAuth } from "../../auth/AuthContext";
import { useWorkspace } from "../../layouts/WorkspaceContext";

const environments = [
  { value: PaymentEnvironment.Test, label: "Test" },
  { value: PaymentEnvironment.Live, label: "Live" }
];

export function PaymentConfigPage() {
  const { client } = useAuth();
  const workspace = useWorkspace();
  const api = paymentsApi(client, workspace.id);
  const query = useQuery({ queryKey: ["payments", workspace.id], queryFn: api.getDodoConfiguration, retry: false });
  const mutation = useMutation({
    mutationFn: api.upsertDodoConfiguration,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["payments", workspace.id] });
      message.success("Payment settings saved");
    },
    onError: (error) => message.error(error instanceof Error ? error.message : "Payment settings failed")
  });

  return (
    <>
      <Typography.Title level={2}>Dodo Payments</Typography.Title>
      <Card>
        {query.data && (
          <Typography.Paragraph>
            Connection status: {query.data.connectionStatus}. API key saved: {query.data.hasApiKey ? "yes" : "no"}.
          </Typography.Paragraph>
        )}
        <Form layout="vertical" initialValues={{ environment: PaymentEnvironment.Test }} onFinish={(values) => mutation.mutate(values)}>
          <Form.Item name="environment" label="Environment"><Select options={environments} /></Form.Item>
          <Form.Item name="apiKey" label="API key" rules={[{ required: true }]}><Input.Password /></Form.Item>
          <Form.Item name="webhookSecret" label="Webhook secret" rules={[{ required: true }]}><Input.Password /></Form.Item>
          <Button loading={mutation.isPending} type="primary" htmlType="submit">Save</Button>
        </Form>
      </Card>
    </>
  );
}
