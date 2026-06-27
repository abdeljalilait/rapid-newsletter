import { Button, Card, Form, Input, Typography, message } from "antd";
import { useMutation } from "@tanstack/react-query";
import { workspacesApi } from "@rapid/api-client";
import { queryClient } from "../../api/queryClient";
import { useAuth } from "../../auth/AuthContext";
import { useWorkspace } from "../../layouts/WorkspaceContext";

export function WorkspaceSettingsPage() {
  const { client } = useAuth();
  const workspace = useWorkspace();
  const api = workspacesApi(client);
  const mutation = useMutation({
    mutationFn: (values: any) => api.update(workspace.id, values),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["workspace", workspace.id] });
      message.success("Workspace updated");
    },
    onError: (error) => message.error(error instanceof Error ? error.message : "Workspace update failed")
  });

  return (
    <>
      <Typography.Title level={2}>Workspace settings</Typography.Title>
      <Card>
        <Form layout="vertical" initialValues={workspace} onFinish={(values) => mutation.mutate(values)}>
          <Form.Item name="name" label="Name" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="description" label="Description"><Input.TextArea rows={3} /></Form.Item>
          <Form.Item name="logoUrl" label="Logo URL"><Input /></Form.Item>
          <Form.Item name="defaultSenderName" label="Default sender name" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="defaultSenderEmail" label="Default sender email" rules={[{ required: true, type: "email" }]}><Input /></Form.Item>
          <Form.Item name="timezone" label="Timezone" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="defaultCurrency" label="Currency" rules={[{ required: true }]}><Input maxLength={3} /></Form.Item>
          <Button loading={mutation.isPending} type="primary" htmlType="submit">Save</Button>
        </Form>
      </Card>
    </>
  );
}
