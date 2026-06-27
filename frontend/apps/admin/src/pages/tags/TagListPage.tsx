import { Button, Form, Input, Modal, Table, Typography, message } from "antd";
import { PlusOutlined } from "@ant-design/icons";
import { useMutation, useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { TagDto, tagsApi } from "@rapid/api-client";
import { queryClient } from "../../api/queryClient";
import { useAuth } from "../../auth/AuthContext";
import { useWorkspace } from "../../layouts/WorkspaceContext";

export function TagListPage() {
  const { client } = useAuth();
  const workspace = useWorkspace();
  const api = tagsApi(client, workspace.id);
  const [open, setOpen] = useState(false);
  const query = useQuery({ queryKey: ["tags", workspace.id], queryFn: api.list });
  const mutation = useMutation({
    mutationFn: (values: { name: string }) => api.create(values.name),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["tags", workspace.id] });
      setOpen(false);
    },
    onError: (error) => message.error(error instanceof Error ? error.message : "Tag creation failed")
  });

  return (
    <>
      <div className="page-header">
        <Typography.Title level={2}>Tags</Typography.Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => setOpen(true)}>Create</Button>
      </div>
      <Table<TagDto> rowKey="id" loading={query.isLoading} dataSource={query.data} columns={[{ title: "Name", dataIndex: "name" }, { title: "Slug", dataIndex: "slug" }]} />
      <Modal title="Create tag" open={open} onCancel={() => setOpen(false)} footer={null}>
        <Form layout="vertical" onFinish={(values) => mutation.mutate(values)}>
          <Form.Item name="name" label="Name" rules={[{ required: true }]}><Input /></Form.Item>
          <Button type="primary" htmlType="submit">Save</Button>
        </Form>
      </Modal>
    </>
  );
}
