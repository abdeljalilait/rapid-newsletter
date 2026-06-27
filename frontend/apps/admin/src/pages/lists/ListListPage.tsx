import { Button, Form, Input, Modal, Table, Typography, message } from "antd";
import { PlusOutlined } from "@ant-design/icons";
import { useMutation, useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { ListDto, listsApi } from "@rapid/api-client";
import { queryClient } from "../../api/queryClient";
import { useAuth } from "../../auth/AuthContext";
import { useWorkspace } from "../../layouts/WorkspaceContext";

export function ListListPage() {
  const { client } = useAuth();
  const workspace = useWorkspace();
  const api = listsApi(client, workspace.id);
  const [open, setOpen] = useState(false);
  const query = useQuery({ queryKey: ["lists", workspace.id], queryFn: api.list });
  const mutation = useMutation({
    mutationFn: api.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["lists", workspace.id] });
      setOpen(false);
    },
    onError: (error) => message.error(error instanceof Error ? error.message : "List creation failed")
  });

  return (
    <>
      <div className="page-header">
        <Typography.Title level={2}>Lists</Typography.Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => setOpen(true)}>Create</Button>
      </div>
      <Table<ListDto> rowKey="id" loading={query.isLoading} dataSource={query.data} columns={[{ title: "Name", dataIndex: "name" }, { title: "Description", dataIndex: "description" }]} />
      <Modal title="Create list" open={open} onCancel={() => setOpen(false)} footer={null}>
        <Form layout="vertical" onFinish={(values) => mutation.mutate(values)}>
          <Form.Item name="name" label="Name" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="description" label="Description"><Input.TextArea rows={3} /></Form.Item>
          <Button type="primary" htmlType="submit">Save</Button>
        </Form>
      </Modal>
    </>
  );
}
