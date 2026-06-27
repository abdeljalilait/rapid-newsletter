import { Button, Form, Input, Typography, message } from "antd";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../../auth/AuthContext";

export function RegisterPage() {
  const { register } = useAuth();
  const navigate = useNavigate();

  return (
    <Form
      layout="vertical"
      onFinish={async (values) => {
        try {
          await register(values);
          navigate("/workspaces", { replace: true });
        } catch (error) {
          message.error(error instanceof Error ? error.message : "Registration failed");
        }
      }}
    >
      <Form.Item name="firstName" label="First name" rules={[{ required: true }]}>
        <Input autoComplete="given-name" />
      </Form.Item>
      <Form.Item name="lastName" label="Last name">
        <Input autoComplete="family-name" />
      </Form.Item>
      <Form.Item name="email" label="Email" rules={[{ required: true, type: "email" }]}>
        <Input autoComplete="email" />
      </Form.Item>
      <Form.Item name="password" label="Password" rules={[{ required: true, min: 8 }]}>
        <Input.Password autoComplete="new-password" />
      </Form.Item>
      <Button type="primary" htmlType="submit" block>
        Register
      </Button>
      <Typography.Paragraph style={{ marginTop: 16 }}>
        Already registered? <Link to="/login">Log in</Link>
      </Typography.Paragraph>
    </Form>
  );
}
