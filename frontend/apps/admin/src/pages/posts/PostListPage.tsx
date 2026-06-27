import { Button, Form, Input, Modal, Select, Switch, Table, Tag, Typography, message } from "antd";
import { PlusOutlined } from "@ant-design/icons";
import { useMutation, useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { postsApi, PostAudience, PostDto, PostStatus } from "@rapid/api-client";
import { queryClient } from "../../api/queryClient";
import { useAuth } from "../../auth/AuthContext";
import { AceHtmlEditor } from "../../components/AceHtmlEditor";
import { enumOptions, postAudienceLabels, postStatusLabels } from "../../components/enumLabels";
import { useWorkspace } from "../../layouts/WorkspaceContext";

export function PostListPage() {
  const { client } = useAuth();
  const workspace = useWorkspace();
  const api = postsApi(client, workspace.id);
  const [open, setOpen] = useState(false);
  const query = useQuery({ queryKey: ["posts", workspace.id], queryFn: api.list });
  const mutation = useMutation({
    mutationFn: api.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["posts", workspace.id] });
      setOpen(false);
    },
    onError: (error) => message.error(error instanceof Error ? error.message : "Post creation failed")
  });

  return (
    <>
      <div className="page-header">
        <Typography.Title level={2}>Posts</Typography.Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => setOpen(true)}>Create</Button>
      </div>
      <Table<PostDto>
        rowKey="id"
        loading={query.isLoading}
        dataSource={query.data}
        columns={[
          { title: "Title", dataIndex: "title" },
          { title: "Slug", dataIndex: "slug" },
          { title: "Audience", dataIndex: "audience", render: (value) => postAudienceLabels[value as PostAudience] },
          { title: "Status", dataIndex: "status", render: (value) => <Tag>{postStatusLabels[value as PostStatus]}</Tag> },
          { title: "Website", dataIndex: "publishOnWebsite", render: (value) => value ? "Yes" : "No" },
          { title: "Email", dataIndex: "sendByEmail", render: (value) => value ? "Yes" : "No" }
        ]}
      />
      <PostModal open={open} onCancel={() => setOpen(false)} onFinish={(values) => mutation.mutate(normalizePost(values))} />
    </>
  );
}

function PostModal(props: { open: boolean; onCancel: () => void; onFinish: (values: ReturnType<typeof normalizePost>) => void }) {
  return (
    <Modal title="Create post" open={props.open} onCancel={props.onCancel} footer={null} width={820}>
      <Form layout="vertical" initialValues={{ audience: PostAudience.Public, status: PostStatus.Draft, publishOnWebsite: true, sendByEmail: true }} onFinish={props.onFinish}>
        <Form.Item name="title" label="Title" rules={[{ required: true }]}><Input /></Form.Item>
        <Form.Item name="slug" label="Slug"><Input placeholder="Auto-generated from title if empty" /></Form.Item>
        <Form.Item name="subtitle" label="Subtitle"><Input /></Form.Item>
        <Form.Item name="previewText" label="Preview text"><Input /></Form.Item>
        <Form.Item name="coverImageUrl" label="Cover image URL"><Input /></Form.Item>
        <Form.Item name="audience" label="Audience"><Select options={enumOptions(postAudienceLabels)} /></Form.Item>
        <Form.Item name="status" label="Status"><Select options={enumOptions(postStatusLabels)} /></Form.Item>
        <Form.Item name="bodyHtml" label="Content" rules={[{ required: true }]}>
          <AceHtmlEditor height="360px" placeholder="<h1>Your post content</h1>" />
        </Form.Item>
        <Form.Item name="publishOnWebsite" label="Publish on website" valuePropName="checked"><Switch /></Form.Item>
        <Form.Item name="sendByEmail" label="Send by email" valuePropName="checked"><Switch /></Form.Item>
        <Button type="primary" htmlType="submit">Save</Button>
      </Form>
    </Modal>
  );
}

function normalizePost(values: any) {
  const bodyHtml = values.bodyHtml || "<p></p>";
  return {
    ...values,
    renderedHtml: bodyHtml,
    editorContentJson: JSON.stringify({ html: bodyHtml }),
    plainText: stripHtml(bodyHtml),
    scheduledAt: null
  };
}

function stripHtml(html: string) {
  return html.replace(/<[^>]*>/g, " ").replace(/\s+/g, " ").trim();
}
