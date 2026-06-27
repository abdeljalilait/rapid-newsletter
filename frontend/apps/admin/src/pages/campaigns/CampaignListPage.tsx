import { Button, Checkbox, Form, Input, InputNumber, Modal, Select, Space, Table, Tag, Typography, message } from "antd";
import { PlusOutlined, SendOutlined } from "@ant-design/icons";
import { useMutation, useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { AceHtmlEditor } from "../../components/AceHtmlEditor";
import {
  CampaignDto,
  CampaignStatus,
  ProviderAccountDto,
  SubscriberAccessLevel,
  SubscriberStatus,
  campaignsApi,
  providersApi
} from "@rapid/api-client";
import { queryClient } from "../../api/queryClient";
import { useAuth } from "../../auth/AuthContext";
import { accessLevelLabels, campaignStatusLabels, enumOptions, providerLabels } from "../../components/enumLabels";
import { useWorkspace } from "../../layouts/WorkspaceContext";

export function CampaignListPage() {
  const { client } = useAuth();
  const workspace = useWorkspace();
  const campaignApi = campaignsApi(client, workspace.id);
  const providerApi = providersApi(client, workspace.id);
  const [createOpen, setCreateOpen] = useState(false);
  const [launching, setLaunching] = useState<CampaignDto | null>(null);
  const campaigns = useQuery({ queryKey: ["campaigns", workspace.id], queryFn: campaignApi.list });
  const providers = useQuery({ queryKey: ["providers", workspace.id], queryFn: providerApi.list });
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ["campaigns", workspace.id] });

  const createMutation = useMutation({
    mutationFn: campaignApi.create,
    onSuccess: () => {
      invalidate();
      setCreateOpen(false);
    },
    onError: showError
  });

  return (
    <>
      <div className="page-header">
        <Typography.Title level={2}>Campaigns</Typography.Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => setCreateOpen(true)}>Create</Button>
      </div>
      <Table<CampaignDto>
        rowKey="id"
        loading={campaigns.isLoading}
        dataSource={campaigns.data}
        columns={[
          { title: "Name", dataIndex: "name" },
          { title: "Subject", dataIndex: "subject" },
          { title: "Recipients", dataIndex: "recipientCount" },
          { title: "Status", dataIndex: "status", render: (value) => <Tag>{campaignStatusLabels[value as CampaignStatus]}</Tag> },
          {
            title: "Actions",
            render: (_, row) => (
              <Button icon={<SendOutlined />} onClick={() => setLaunching(row)}>
                Launch
              </Button>
            )
          }
        ]}
      />
      <CampaignModal open={createOpen} onCancel={() => setCreateOpen(false)} onFinish={(values) => createMutation.mutate(normalizeCampaign(values))} />
      {launching && (
        <LaunchModal
          campaign={launching}
          providers={providers.data ?? []}
          onCancel={() => setLaunching(null)}
          onLaunched={() => {
            setLaunching(null);
            invalidate();
          }}
        />
      )}
    </>
  );
}

function CampaignModal(props: { open: boolean; onCancel: () => void; onFinish: (values: any) => void }) {
  return (
    <Modal title="Create campaign" open={props.open} onCancel={props.onCancel} footer={null} width={760}>
      <Form layout="vertical" initialValues={{ accessLevel: SubscriberAccessLevel.Free, allowPartialCampaign: false }} onFinish={props.onFinish}>
        <Form.Item name="name" label="Name" rules={[{ required: true }]}><Input /></Form.Item>
        <Form.Item name="subject" label="Subject" rules={[{ required: true }]}><Input /></Form.Item>
        <Form.Item name="previewText" label="Preview text"><Input /></Form.Item>
        <Form.Item name="fromName" label="From name" rules={[{ required: true }]}><Input /></Form.Item>
        <Form.Item name="fromEmail" label="From email" rules={[{ required: true, type: "email" }]}><Input /></Form.Item>
        <Form.Item name="replyTo" label="Reply-to"><Input /></Form.Item>
        <Form.Item name="accessLevel" label="Audience access"><Select options={enumOptions(accessLevelLabels)} /></Form.Item>
        <Form.Item name="bodyHtml" label="Body HTML" rules={[{ required: true }]}>
          <AceHtmlEditor height="360px" placeholder="<h1>Your campaign HTML</h1>" />
        </Form.Item>
        <Form.Item name="allowPartialCampaign" valuePropName="checked"><Checkbox>Allow partial campaign</Checkbox></Form.Item>
        <Button type="primary" htmlType="submit">Save</Button>
      </Form>
    </Modal>
  );
}

function LaunchModal(props: { campaign: CampaignDto; providers: ProviderAccountDto[]; onCancel: () => void; onLaunched: () => void }) {
  const { client } = useAuth();
  const workspace = useWorkspace();
  const api = campaignsApi(client, workspace.id);
  const [form] = Form.useForm();
  const [estimate, setEstimate] = useState<any>(null);
  const estimateMutation = useMutation({ mutationFn: (values: any) => api.estimate(props.campaign.id, normalizeLaunch(values)), onSuccess: setEstimate, onError: showError });
  const launchMutation = useMutation({
    mutationFn: (values: any) => api.launch(props.campaign.id, normalizeLaunch(values)),
    onSuccess: () => {
      message.success("Campaign launched");
      props.onLaunched();
    },
    onError: showError
  });

  return (
    <Modal title={`Launch ${props.campaign.name}`} open onCancel={props.onCancel} footer={null} width={860}>
      <Form form={form} layout="vertical" initialValues={{ allowPartialCampaign: props.campaign.allowPartialCampaign }} onFinish={(values) => launchMutation.mutate(values)}>
        <Form.List name="providerAccounts">
          {(fields, { add, remove }) => (
            <>
              {fields.map((field) => (
                <Space key={field.key} align="baseline" wrap>
                  <Form.Item {...field} name={[field.name, "providerAccountId"]} rules={[{ required: true }]}>
                    <Select style={{ width: 220 }} placeholder="Provider" options={providerOptions(props.providers)} />
                  </Form.Item>
                  <Form.Item {...field} name={[field.name, "priority"]} initialValue={1}><InputNumber min={1} placeholder="Priority" /></Form.Item>
                  <Form.Item {...field} name={[field.name, "ratePerMinute"]} initialValue={1}><InputNumber min={1} placeholder="Rate/min" /></Form.Item>
                  <Form.Item {...field} name={[field.name, "maximumEmails"]}><InputNumber min={1} placeholder="Max" /></Form.Item>
                  <Button onClick={() => remove(field.name)}>Remove</Button>
                </Space>
              ))}
              <Button onClick={() => add({ priority: 1, ratePerMinute: 1, enabled: true })}>Add account</Button>
            </>
          )}
        </Form.List>
        <Form.Item name="allowPartialCampaign" valuePropName="checked" style={{ marginTop: 16 }}>
          <Checkbox>Allow partial campaign</Checkbox>
        </Form.Item>
        {estimate && <Typography.Paragraph>Recipients {estimate.finalRecipients}, capacity {estimate.totalSelectedCapacity}, missing {estimate.missingCapacity}, rate {estimate.combinedRatePerMinute}/min</Typography.Paragraph>}
        <Space>
          <Button onClick={() => form.validateFields().then((values) => estimateMutation.mutate(values))}>Estimate</Button>
          <Button loading={launchMutation.isPending} type="primary" htmlType="submit">Launch</Button>
        </Space>
      </Form>
    </Modal>
  );
}

function normalizeCampaign(values: any) {
  return {
    ...values,
    postId: null,
    plainText: String(values.bodyHtml).replace(/<[^>]*>/g, " "),
    scheduledAt: null,
    audienceFilter: { status: SubscriberStatus.Active, accessLevel: values.accessLevel }
  };
}

function normalizeLaunch(values: any) {
  return {
    allowPartialCampaign: Boolean(values.allowPartialCampaign),
    providerAccounts: (values.providerAccounts ?? []).map((account: any) => ({ ...account, enabled: true }))
  };
}

function providerOptions(providers: ProviderAccountDto[]) {
  return providers.map((provider) => ({
    value: provider.id,
    label: `${provider.accountName} (${providerLabels[provider.provider]})`
  }));
}

function showError(error: unknown) {
  message.error(error instanceof Error ? error.message : "Request failed");
}
