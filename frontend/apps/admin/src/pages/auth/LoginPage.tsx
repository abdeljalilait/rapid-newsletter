import { Button, Form, Input, Typography, message } from "antd";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "../../auth/AuthContext";

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  return (
    <Form
      layout="vertical"
      onFinish={async (values) => {
        try {
          await login(values.email, values.password);
          navigate((location.state as { from?: Location })?.from?.pathname ?? "/workspaces", { replace: true });
        } catch (error) {
          message.error(error instanceof Error ? error.message : "Login failed");
        }
      }}
    >
      <Form.Item name="email" label="Email" rules={[{ required: true, type: "email" }]}>
        <Input autoComplete="email" />
      </Form.Item>
      <Form.Item name="password" label="Password" rules={[{ required: true }]}>
        <Input.Password autoComplete="current-password" />
      </Form.Item>
      <Button type="primary" htmlType="submit" block>
        Log in
      </Button>
      <Typography.Paragraph style={{ marginTop: 16 }}>
        No account yet? <Link to="/register">Register</Link>
      </Typography.Paragraph>
    </Form>
  );
}
