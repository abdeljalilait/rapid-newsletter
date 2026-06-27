import { Button, Card, Form, Input, Modal, Space, Typography, message } from "antd";
import { PlusOutlined } from "@ant-design/icons";
import { useMutation, useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { workspacesApi } from "@rapid/api-client";
import { queryClient } from "../../api/queryClient";
import { useAuth } from "../../auth/AuthContext";

export function WorkspaceListPage() {
  const { client } = useAuth();
  const api = workspacesApi(client);
  const navigate = useNavigate();
  const [open, setOpen] = useState(false);
  const query = useQuery({ queryKey: ["workspaces"], queryFn: api.list });
  const mutation = useMutation({
    mutationFn: api.create,
    onSuccess: (workspace) => {
      queryClient.invalidateQueries({ queryKey: ["workspaces"] });
      setOpen(false);
      navigate(`/${workspace.id}/dashboard`);
    },
    onError: (error) => message.error(error instanceof Error ? error.message : "Workspace creation failed")
  });

  return (
    <>
      <div className="page-header">
        <Typography.Title level={2}>Workspaces</Typography.Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => setOpen(true)}>
          Create
        </Button>
      </div>
      <Space direction="vertical" size={12} style={{ width: "100%" }}>
        {query.data?.map((workspace) => (
          <Card key={workspace.id} hoverable onClick={() => navigate(`/${workspace.id}/dashboard`)}>
            <Typography.Title level={4} style={{ marginTop: 0 }}>{workspace.name}</Typography.Title>
            <Typography.Text type="secondary">/{workspace.slug} · {workspace.currentUserRole}</Typography.Text>
          </Card>
        ))}
      </Space>
      <Modal title="Create workspace" open={open} onCancel={() => setOpen(false)} footer={null}>
        <Form layout="vertical" onFinish={(values) => mutation.mutate(values)} initialValues={{ timezone: "UTC", defaultCurrency: "USD" }}>
          <Form.Item name="name" label="Name" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="slug" label="Slug"><Input /></Form.Item>
          <Form.Item name="description" label="Description"><Input.TextArea rows={3} /></Form.Item>
          <Form.Item name="defaultSenderName" label="Default sender name" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="defaultSenderEmail" label="Default sender email" rules={[{ required: true, type: "email" }]}><Input /></Form.Item>
          <Form.Item name="timezone" label="Timezone"><Input /></Form.Item>
          <Form.Item name="defaultCurrency" label="Currency"><Input maxLength={3} /></Form.Item>
          <Button loading={mutation.isPending} type="primary" htmlType="submit">Create</Button>
        </Form>
      </Modal>
    </>
  );
}
