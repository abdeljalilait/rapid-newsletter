import { Button, Form, Input, InputNumber, Modal, Select, Switch, Table, Typography, message } from "antd";
import { PlusOutlined } from "@ant-design/icons";
import { useMutation, useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { BillingInterval, PlanDto, plansApi } from "@rapid/api-client";
import { queryClient } from "../../api/queryClient";
import { useAuth } from "../../auth/AuthContext";
import { billingIntervalLabels, enumOptions } from "../../components/enumLabels";
import { useWorkspace } from "../../layouts/WorkspaceContext";

export function PlanListPage() {
  const { client } = useAuth();
  const workspace = useWorkspace();
  const api = plansApi(client, workspace.id);
  const [open, setOpen] = useState(false);
  const query = useQuery({ queryKey: ["plans", workspace.id], queryFn: api.list });
  const mutation = useMutation({
    mutationFn: (values: any) => api.create({ ...values, benefits: parseBenefits(values.benefits) }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["plans", workspace.id] });
      setOpen(false);
    },
    onError: (error) => message.error(error instanceof Error ? error.message : "Plan creation failed")
  });

  return (
    <>
      <div className="page-header">
        <Typography.Title level={2}>Plans</Typography.Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => setOpen(true)}>Create</Button>
      </div>
      <Table<PlanDto>
        rowKey="id"
        loading={query.isLoading}
        dataSource={query.data}
        columns={[
          { title: "Name", dataIndex: "name" },
          { title: "Price", render: (_, row) => `${row.currency} ${row.price}` },
          { title: "Interval", dataIndex: "billingInterval", render: (value) => billingIntervalLabels[value as BillingInterval] },
          { title: "Active", dataIndex: "isActive", render: (value) => value ? "Yes" : "No" }
        ]}
      />
      <Modal title="Create plan" open={open} onCancel={() => setOpen(false)} footer={null}>
        <Form layout="vertical" initialValues={{ currency: workspace.defaultCurrency, billingInterval: BillingInterval.Monthly, isActive: true, sortOrder: 0 }} onFinish={(values) => mutation.mutate(values)}>
          <Form.Item name="name" label="Name" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="description" label="Description"><Input.TextArea rows={2} /></Form.Item>
          <Form.Item name="price" label="Price" rules={[{ required: true }]}><InputNumber min={0} style={{ width: "100%" }} /></Form.Item>
          <Form.Item name="currency" label="Currency"><Input maxLength={3} /></Form.Item>
          <Form.Item name="billingInterval" label="Interval"><Select options={enumOptions(billingIntervalLabels)} /></Form.Item>
          <Form.Item name="dodoProductId" label="Dodo product ID"><Input /></Form.Item>
          <Form.Item name="benefits" label="Benefits"><Input.TextArea rows={3} placeholder="One benefit per line" /></Form.Item>
          <Form.Item name="isActive" label="Active" valuePropName="checked"><Switch /></Form.Item>
          <Form.Item name="sortOrder" label="Sort order"><InputNumber min={0} /></Form.Item>
          <Button type="primary" htmlType="submit">Save</Button>
        </Form>
      </Modal>
    </>
  );
}

function parseBenefits(value?: string) {
  return value?.split("\n").map((line) => line.trim()).filter(Boolean) ?? [];
}
