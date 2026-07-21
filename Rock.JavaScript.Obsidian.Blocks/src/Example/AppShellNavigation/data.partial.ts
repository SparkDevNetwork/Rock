// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

// =============================================================================
// STATIC DEMO DATA - NOT backed by Rock.
// =============================================================================
// These arrays feed the app-shell widgets that have no Rock data source yet:
// the notifications bell, the project progress rings, the spending meter, and
// the "Organizations" switcher. They exist purely so the shell reproduces the
// full app-shell look. The real navigation tree and the real user menu come from the
// block's initialization box, not from this file. Do not mistake any of this
// for live data.
// =============================================================================

export type Workspace = {
    id: string;
    name: string;
    tier?: string;
};

export type Project = {
    id: string;
    name: string;
    progress: number;
    /** A CSS color value (resolves against the theme tokens). */
    color: string;
};

export type ItemAction = {
    id: string;
    label: string;
    iconClass: string;
    isDestructive: boolean;
};

export const workspaces: Workspace[] = [
    { id: "claude", name: "Claude", tier: "Enterprise" },
    { id: "vercel", name: "Vercel", tier: "Pro" },
    { id: "openai", name: "OpenAI", tier: "Team" }
];

export const activeProjects: Project[] = [
    { id: "design-sys", name: "Design System", progress: 72, color: "var(--color-info-strong)" },
    { id: "api-int", name: "API Integration", progress: 45, color: "var(--color-primary)" },
    { id: "mobile-app", name: "Mobile App", progress: 88, color: "var(--color-success-strong)" },
    { id: "analytics", name: "Analytics Dashboard", progress: 30, color: "var(--color-warning-strong)" },
    { id: "auth-mod", name: "Auth Module", progress: 60, color: "var(--color-danger-strong)" }
];

export const itemActions: ItemAction[] = [
    { id: "open", label: "Open Project", iconClass: "ti ti-external-link", isDestructive: false },
    { id: "assign", label: "Assign Members", iconClass: "ti ti-user-plus", isDestructive: false },
    { id: "milestone", label: "Set Milestone", iconClass: "ti ti-flag", isDestructive: false },
    { id: "duplicate", label: "Duplicate", iconClass: "ti ti-copy", isDestructive: false },
    { id: "archive", label: "Archive", iconClass: "ti ti-archive", isDestructive: true }
];

/** Returns the single-letter fallback used inside a workspace avatar. */
export function getWorkspaceInitial(name: string): string {
    return name.charAt(0).toUpperCase();
}

export type NotificationVariant = "info" | "success" | "warning" | "destructive" | "default";

export type NotificationAction = {
    label: string;
    variant: "primary" | "outline";
};

export type Notification = {
    id: string;
    iconClass: string;
    variant: NotificationVariant;
    title: string;
    body?: string;
    time: string;
    isUnread: boolean;
    badge?: string;
    meta?: { label: string; value: string };
    actions?: NotificationAction[];
};

// A representative subset of the notification shapes.
export const notifications: Notification[] = [
    {
        id: "n1",
        iconClass: "ti ti-message",
        variant: "default",
        title: "Sarah mentioned you",
        body: "\"Can you review the changes?\" in #PR-1024",
        time: "2m ago",
        isUnread: false
    },
    {
        id: "n2",
        iconClass: "ti ti-checks",
        variant: "warning",
        title: "Pending approval",
        body: "Design System v2.0 release requires your approval before deployment.",
        time: "5m ago",
        isUnread: true,
        meta: { label: "Priority", value: "High" },
        actions: [
            { label: "Approve", variant: "primary" },
            { label: "Review", variant: "outline" }
        ]
    },
    {
        id: "n3",
        iconClass: "ti ti-circle-check",
        variant: "info",
        title: "Task assigned to you",
        body: "Implement user authentication flow for the mobile app.",
        time: "30m ago",
        isUnread: true,
        meta: { label: "Due", value: "Tomorrow" }
    },
    {
        id: "n4",
        iconClass: "ti ti-users",
        variant: "success",
        title: "4 people joined your workspace",
        body: "Sarah, Mike, Emma and James just joined Pro.",
        time: "45m ago",
        isUnread: true
    },
    {
        id: "n5",
        iconClass: "ti ti-user-plus",
        variant: "info",
        title: "Team invitation",
        body: "Alex invited you to join Pro.",
        time: "1h ago",
        isUnread: false,
        actions: [
            { label: "Accept", variant: "primary" },
            { label: "Decline", variant: "outline" }
        ]
    },
    {
        id: "n6",
        iconClass: "ti ti-credit-card",
        variant: "info",
        title: "Payment processed",
        body: "Your monthly subscription was renewed.",
        time: "3h ago",
        isUnread: false,
        badge: "$49.00"
    },
    {
        id: "n7",
        iconClass: "ti ti-shield-lock",
        variant: "destructive",
        title: "New sign-in detected",
        body: "We noticed a new login from Mac OS, Chrome.",
        time: "Yesterday",
        isUnread: false
    },
    {
        id: "n8",
        iconClass: "ti ti-rocket",
        variant: "success",
        title: "Deployment successful",
        body: "Production branch deployed to Vercel.",
        time: "4 days ago",
        isUnread: false,
        meta: { label: "Env", value: "Production" }
    }
];
