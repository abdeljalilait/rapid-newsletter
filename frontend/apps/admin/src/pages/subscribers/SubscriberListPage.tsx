import { Button, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import { ImportOutlined, PlusOutlined } from "@ant-design/icons";
import { useMutation, useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { subscribersApi, SubscriberAccessLevel, SubscriberStatus, SubscriberDto } from "@rapid/api-client";
import { queryClient } from "../../api/queryClient";
import { useAuth } from "../../auth/AuthContext";
import { accessLevelLabels, enumOptions, subscriberStatusLabels } from "../../components/enumLabels";
import { useWorkspace } from "../../layouts/WorkspaceContext";

export function SubscriberListPage() {
  const { client } = useAuth();
  const workspace = useWorkspace();
  const api = subscribersApi(client, workspace.id);
  const [createOpen, setCreateOpen] = useState(false);
  const [importOpen, setImportOpen] = useState(false);
  const query = useQuery({ queryKey: ["subscribers", workspace.id], queryFn: api.list });
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ["subscribers", workspace.id] });

  const createMutation = useMutation({
    mutationFn: api.create,
    onSuccess: () => {
      invalidate();
      setCreateOpen(false);
    },
    onError: showError
  });

  const importMutation = useMutation({
    mutationFn: api.importJson,
    onSuccess: (summary) => {
      invalidate();
      setImportOpen(false);
      message.success(`${summary.imported} imported, ${summary.duplicates} duplicates, ${summary.invalid} invalid`);
    },
    onError: showError
  });

  return (
    <>
      <div className="page-header">
        <Typography.Title level={2}>Subscribers</Typography.Title>
        <div className="page-actions">
          <Button icon={<ImportOutlined />} onClick={() => setImportOpen(true)}>Import JSON</Button>
          <Button type="primary" icon={<PlusOutlined />} onClick={() => setCreateOpen(true)}>Add</Button>
        </div>
      </div>
      <Table<SubscriberDto>
        rowKey="id"
        loading={query.isLoading}
        dataSource={query.data}
        columns={[
          { title: "Email", dataIndex: "email" },
          { title: "Name", render: (_, row) => [row.firstName, row.lastName].filter(Boolean).join(" ") || "—" },
          { title: "Status", dataIndex: "status", render: (value) => <Tag>{subscriberStatusLabels[value as SubscriberStatus]}</Tag> },
          { title: "Access", dataIndex: "accessLevel", render: (value) => accessLevelLabels[value as SubscriberAccessLevel] },
          { title: "Subscribed", dataIndex: "subscribedAt", render: (value) => new Date(value).toLocaleDateString() }
        ]}
      />
      <SubscriberModal open={createOpen} onCancel={() => setCreateOpen(false)} onFinish={(values) => createMutation.mutate(values)} />
      <ImportModal open={importOpen} loading={importMutation.isPending} onCancel={() => setImportOpen(false)} onImport={(rows) => importMutation.mutate(rows)} />
    </>
  );
}

function SubscriberModal({ open, onCancel, onFinish }: { open: boolean; onCancel: () => void; onFinish: (values: any) => void }) {
  return (
    <Modal title="Add subscriber" open={open} onCancel={onCancel} footer={null}>
      <Form layout="vertical" initialValues={{ status: SubscriberStatus.Active, accessLevel: SubscriberAccessLevel.Free }} onFinish={onFinish}>
        <Form.Item name="email" label="Email" rules={[{ required: true, type: "email" }]}><Input /></Form.Item>
        <Space.Compact block>
          <Form.Item name="firstName" label="First name" style={{ width: "50%" }}><Input /></Form.Item>
          <Form.Item name="lastName" label="Last name" style={{ width: "50%" }}><Input /></Form.Item>
        </Space.Compact>
        <Form.Item name="status" label="Status"><Select options={enumOptions(subscriberStatusLabels)} /></Form.Item>
        <Form.Item name="accessLevel" label="Access"><Select options={enumOptions(accessLevelLabels)} /></Form.Item>
        <Form.Item name="consentSource" label="Consent source"><Input /></Form.Item>
        <Button type="primary" htmlType="submit">Save</Button>
      </Form>
    </Modal>
  );
}

function ImportModal(props: { open: boolean; loading: boolean; onCancel: () => void; onImport: (rows: any[]) => void }) {
  return (
    <Modal title="Import subscribers" open={props.open} onCancel={props.onCancel} footer={null}>
      <Form layout="vertical" onFinish={(values) => props.onImport(JSON.parse(values.rows))}>
        <Form.Item name="rows" label="Rows JSON" rules={[{ required: true }]}>
          <Input.TextArea rows={8} placeholder='[{"email":"reader@example.com","accessLevel":0}]' />
        </Form.Item>
        <Button loading={props.loading} type="primary" htmlType="submit">Import</Button>
      </Form>
    </Modal>
  );
}

function showError(error: unknown) {
  message.error(error instanceof Error ? error.message : "Request failed");
}
