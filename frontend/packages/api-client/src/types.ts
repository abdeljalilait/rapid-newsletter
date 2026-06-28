export type Guid = string;

export enum SubscriberStatus {
  PendingConfirmation = 0,
  Active = 1,
  Unsubscribed = 2,
  HardBounced = 3,
  SoftBounced = 4,
  Complained = 5,
  Suppressed = 6,
  Invalid = 7
}

export enum SubscriberAccessLevel {
  Free = 0,
  Paid = 1,
  Complimentary = 2,
  Lifetime = 3
}

export enum BillingInterval {
  Free = 0,
  Monthly = 1,
  Annual = 2,
  OneTime = 3
}

export enum PaymentEnvironment {
  Test = 0,
  Live = 1
}

export enum PostAudience {
  Public = 0,
  FreeSubscribers = 1,
  PaidSubscribers = 2,
  SpecificList = 3,
  SpecificTags = 4,
  CustomFilter = 5
}

export enum PostStatus {
  Draft = 0,
  Scheduled = 1,
  Published = 2,
  Archived = 3
}

export enum CampaignStatus {
  Draft = 0,
  Preparing = 1,
  Scheduled = 2,
  Sending = 3,
  Paused = 4,
  Completed = 5,
  PartiallyCompleted = 6,
  Cancelled = 7,
  Failed = 8
}

export enum EmailProvider {
  Resend = 0,
  Mailtrap = 1,
  MailerSend = 8,
  Plunk = 9,
  AmazonSes = 10
}

export interface AuthResponse {
  userId: Guid;
  email: string;
  firstName: string;
  lastName?: string | null;
  emailConfirmed: boolean;
  accessToken: string;
  refreshToken: string;
  refreshTokenExpiresAt: string;
  accessTokenExpiresIn: string;
}

export interface WorkspaceDto {
  id: Guid;
  name: string;
  slug: string;
  logoUrl?: string | null;
  description?: string | null;
  defaultSenderName: string;
  defaultSenderEmail: string;
  timezone: string;
  defaultCurrency: string;
  status: string;
  currentUserRole: string;
  createdAt: string;
}

export interface PublicWorkspaceDto {
  id: Guid;
  name: string;
  slug: string;
  logoUrl?: string | null;
  description?: string | null;
}

export interface SubscriberDto {
  id: Guid;
  email: string;
  firstName?: string | null;
  lastName?: string | null;
  status: SubscriberStatus;
  accessLevel: SubscriberAccessLevel;
  subscribedAt: string;
}

export interface PublicUnsubscribeDto {
  subscriberId: Guid;
  email: string;
  status: SubscriberStatus;
  suppressed: boolean;
}

export interface TagDto {
  id: Guid;
  name: string;
  slug: string;
}

export interface ListDto {
  id: Guid;
  name: string;
  description?: string | null;
}

export interface PlanDto {
  id: Guid;
  name: string;
  description?: string | null;
  price: number;
  currency: string;
  billingInterval: BillingInterval;
  dodoProductId?: string | null;
  isActive: boolean;
  sortOrder: number;
}

export interface PaymentConfigurationDto {
  id: Guid;
  environment: PaymentEnvironment;
  connectionStatus: number;
  lastValidatedAt?: string | null;
  hasApiKey: boolean;
  hasWebhookSecret: boolean;
}

export interface ProviderAccountDto {
  id: Guid;
  provider: EmailProvider;
  accountName: string;
  fromName: string;
  fromEmail: string;
  sendingDomain?: string | null;
  dailyLimit?: number | null;
  monthlyLimit?: number | null;
  ratePerMinute: number;
  enabled: boolean;
  healthStatus: number;
  lastValidatedAt?: string | null;
  lastSuccessfulSendAt?: string | null;
  lastError?: string | null;
  hasApiKey: boolean;
  hasApiSecret: boolean;
}

export interface PostDto {
  id: Guid;
  title: string;
  slug: string;
  subtitle?: string | null;
  previewText?: string | null;
  coverImageUrl?: string | null;
  audience: PostAudience;
  status: PostStatus;
  publishOnWebsite: boolean;
  sendByEmail: boolean;
  scheduledAt?: string | null;
  publishedAt?: string | null;
}

export interface PublicPostDetailDto {
  id: Guid;
  title: string;
  slug: string;
  subtitle?: string | null;
  previewText?: string | null;
  coverImageUrl?: string | null;
  audience: PostAudience;
  status: PostStatus;
  publishedAt?: string | null;
  renderedHtml?: string | null;
  plainText?: string | null;
}

export interface AudienceFilterDto {
  status?: SubscriberStatus | null;
  accessLevel?: SubscriberAccessLevel | null;
  tagIds?: Guid[] | null;
  listIds?: Guid[] | null;
  joinedFrom?: string | null;
  joinedTo?: string | null;
}

export interface CampaignDto {
  id: Guid;
  postId?: Guid | null;
  name: string;
  subject: string;
  status: CampaignStatus;
  scheduledAt?: string | null;
  allowPartialCampaign: boolean;
  recipientCount: number;
}

export interface ProviderCapacityDto {
  providerAccountId: Guid;
  accountName: string;
  remainingCapacity: number;
  ratePerMinute: number;
}

export interface CampaignCapacityDto {
  finalRecipients: number;
  totalSelectedCapacity: number;
  missingCapacity: number;
  combinedRatePerMinute: number;
  accounts: ProviderCapacityDto[];
}

export interface OverviewAnalyticsDto {
  totalSubscribers: number;
  freeSubscribers: number;
  paidSubscribers: number;
  activeSubscriptions: number;
  emailsSentThisMonth: number;
}

export interface ImportSummaryDto {
  rowsUploaded: number;
  imported: number;
  duplicates: number;
  invalid: number;
  skipped: number;
}

export interface SelectedProviderAccountRequest {
  providerAccountId: Guid;
  priority: number;
  ratePerMinute: number;
  maximumEmails?: number | null;
  enabled: boolean;
}

export interface CampaignLaunchRequest {
  providerAccounts: SelectedProviderAccountRequest[];
  allowPartialCampaign: boolean;
}
