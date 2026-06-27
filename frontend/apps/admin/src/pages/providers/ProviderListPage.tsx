import { Button, Form, Input, InputNumber, Modal, Select, Switch, Table, Tag, Typography, message } from "antd";
import { CheckCircleOutlined, PlusOutlined } from "@ant-design/icons";
import { useMutation, useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { EmailProvider, ProviderAccountDto, providersApi } from "@rapid/api-client";
import { queryClient } from "../../api/queryClient";
import { useAuth } from "../../auth/AuthContext";
import { enumOptions, providerLabels } from "../../components/enumLabels";
import { useWorkspace } from "../../layouts/WorkspaceContext";

export function ProviderListPage() {
  const { client } = useAuth();
  const workspace = useWorkspace();
  const api = providersApi(client, workspace.id);
  const [open, setOpen] = useState(false);
  const query = useQuery({ queryKey: ["providers", workspace.id], queryFn: api.list });
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ["providers", workspace.id] });
  const createMutation = useMutation({
    mutationFn: api.create,
    onSuccess: () => {
      invalidate();
      setOpen(false);
    },
    onError: showError
  });
  const validateMutation = useMutation({
    mutationFn: api.validate,
    onSuccess: () => {
      invalidate();
      message.success("Provider validated");
    },
    onError: showError
  });

  return (
    <>
      <div className="page-header">
        <Typography.Title level={2}>Provider accounts</Typography.Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => setOpen(true)}>Add</Button>
      </div>
      <Table<ProviderAccountDto>
        rowKey="id"
        loading={query.isLoading}
        dataSource={query.data}
        columns={[
          { title: "Account", dataIndex: "accountName" },
          { title: "Provider", dataIndex: "provider", render: (value) => providerLabels[value as EmailProvider] },
          { title: "From", render: (_, row) => `${row.fromName} <${row.fromEmail}>` },
          { title: "Rate/min", dataIndex: "ratePerMinute" },
          { title: "Status", dataIndex: "healthStatus", render: (value) => <Tag>{value}</Tag> },
          {
            title: "Actions",
            render: (_, row) => (
              <Button icon={<CheckCircleOutlined />} onClick={() => validateMutation.mutate(row.id)}>
                Validate
              </Button>
            )
          }
        ]}
      />
      <Modal title="Add provider account" open={open} onCancel={() => setOpen(false)} footer={null}>
        <Form layout="vertical" initialValues={{ provider: EmailProvider.Resend, ratePerMinute: 1, enabled: true }} onFinish={(values) => createMutation.mutate(values)}>
          <Form.Item name="provider" label="Provider"><Select options={enumOptions(providerLabels)} /></Form.Item>
          <Form.Item name="accountName" label="Account name" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="apiKey" label="API key" rules={[{ required: true }]}><Input.Password /></Form.Item>
          <Form.Item name="fromName" label="From name" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="fromEmail" label="From email" rules={[{ required: true, type: "email" }]}><Input /></Form.Item>
          <Form.Item name="sendingDomain" label="Sending domain"><Input /></Form.Item>
          <Form.Item name="dailyLimit" label="Daily limit"><InputNumber min={0} style={{ width: "100%" }} /></Form.Item>
          <Form.Item name="monthlyLimit" label="Monthly limit"><InputNumber min={0} style={{ width: "100%" }} /></Form.Item>
          <Form.Item name="ratePerMinute" label="Rate per minute"><InputNumber min={1} style={{ width: "100%" }} /></Form.Item>
          <Form.Item name="enabled" label="Enabled" valuePropName="checked"><Switch /></Form.Item>
          <Button type="primary" htmlType="submit">Save</Button>
        </Form>
      </Modal>
    </>
  );
}

function showError(error: unknown) {
  message.error(error instanceof Error ? error.message : "Request failed");
}
