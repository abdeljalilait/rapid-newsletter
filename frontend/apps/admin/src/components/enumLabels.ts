import {
  BillingInterval,
  CampaignStatus,
  EmailProvider,
  PostAudience,
  PostStatus,
  SubscriberAccessLevel,
  SubscriberStatus
} from "@rapid/api-client";

export const subscriberStatusLabels = {
  [SubscriberStatus.PendingConfirmation]: "Pending",
  [SubscriberStatus.Active]: "Active",
  [SubscriberStatus.Unsubscribed]: "Unsubscribed",
  [SubscriberStatus.HardBounced]: "Hard bounced",
  [SubscriberStatus.SoftBounced]: "Soft bounced",
  [SubscriberStatus.Complained]: "Complained",
  [SubscriberStatus.Suppressed]: "Suppressed",
  [SubscriberStatus.Invalid]: "Invalid"
};

export const accessLevelLabels = {
  [SubscriberAccessLevel.Free]: "Free",
  [SubscriberAccessLevel.Paid]: "Paid",
  [SubscriberAccessLevel.Complimentary]: "Complimentary",
  [SubscriberAccessLevel.Lifetime]: "Lifetime"
};

export const billingIntervalLabels = {
  [BillingInterval.Free]: "Free",
  [BillingInterval.Monthly]: "Monthly",
  [BillingInterval.Annual]: "Annual",
  [BillingInterval.OneTime]: "One time"
};

export const postAudienceLabels = {
  [PostAudience.Public]: "Public",
  [PostAudience.FreeSubscribers]: "Free",
  [PostAudience.PaidSubscribers]: "Paid",
  [PostAudience.SpecificList]: "List",
  [PostAudience.SpecificTags]: "Tags",
  [PostAudience.CustomFilter]: "Custom"
};

export const postStatusLabels = {
  [PostStatus.Draft]: "Draft",
  [PostStatus.Scheduled]: "Scheduled",
  [PostStatus.Published]: "Published",
  [PostStatus.Archived]: "Archived"
};

export const campaignStatusLabels = {
  [CampaignStatus.Draft]: "Draft",
  [CampaignStatus.Preparing]: "Preparing",
  [CampaignStatus.Scheduled]: "Scheduled",
  [CampaignStatus.Sending]: "Sending",
  [CampaignStatus.Paused]: "Paused",
  [CampaignStatus.Completed]: "Completed",
  [CampaignStatus.PartiallyCompleted]: "Partial",
  [CampaignStatus.Cancelled]: "Cancelled",
  [CampaignStatus.Failed]: "Failed"
};

export const providerLabels = {
  [EmailProvider.Resend]: "Resend",
  [EmailProvider.Mailtrap]: "Mailtrap",
  [EmailProvider.Sender]: "Sender",
  [EmailProvider.Brevo]: "Brevo",
  [EmailProvider.Mailjet]: "Mailjet",
  [EmailProvider.Mailgun]: "Mailgun",
  [EmailProvider.Loops]: "Loops",
  [EmailProvider.Smtp2Go]: "SMTP2GO"
};

export function enumOptions<T extends Record<number, string>>(labels: T) {
  return Object.entries(labels).map(([value, label]) => ({ value: Number(value), label }));
}
