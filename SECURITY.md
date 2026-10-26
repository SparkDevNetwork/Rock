# Security Policy

Rock RMS holds some of the most sensitive information a church keeps: family records, giving history, attendance, and information about children. This page explains how we protect it and how to report a vulnerability if you find one.

The full, up-to-date policy lives at [rockrms.com/security](https://www.rockrms.com/security).

## Reporting a Vulnerability

If you believe you've found a security vulnerability in Rock RMS, please report it directly through **[Talos](https://security.rockrms.com/)**, our security reporting tool, rather than a public GitHub issue or a public forum. Talos gives you a private, two-way conversation with our team and lets you see your report's status as it moves from investigation to fix to release.

**[Make a Report](https://security.rockrms.com/)**

### Response Timeline

- **Acknowledged** within 1-2 business days
- **Assessed** within 3-5 business days

These times are conservative ceilings. We aim to move faster in practice, but we'd rather commit to a number we consistently beat than one we sometimes miss.

### What to Include

- A description of the vulnerability and its potential impact
- The Rock RMS version(s) affected
- Step-by-step reproduction instructions
- A proof of concept, if you have one
- Your contact information, so we can follow up

### Please Don't

- File public GitHub issues or pull requests for security reports
- Disclose details publicly (RocketChat, social media, blog posts, talks) before we've confirmed the issue and begun remediation
- Share report details with third parties without our authorization

## Our Disclosure Process

Our process is built around one goal: protect churches that haven't updated yet, without leaving anyone in the dark.

1. **We confirm and reproduce.** Our team triages your report in Talos and reproduces the issue to confirm its impact.
2. **We assign a tracking ID.** Once we've confirmed and reproduced the issue, your report gets a tracking ID in Talos, and we share it with you right away. The details stay private while we work on the fix.
3. **We fix and patch.** We develop and test the fix in a private fork, then ship it in a scheduled release, or in an emergency patch for severe issues or environments under active attack.
4. **We publish a security update.** We publish the security update, which shows up as an alert in your Rock instance. It's general in detail, giving churches time to update.

## How We Tell You About Security Updates

We talk to our community directly, so you hear about security fixes from us, where you already work.

- **A banner inside Rock.** If you're a Rock administrator on a version that isn't secured, you'll see a red banner at the top of your internal homepage. It can't be dismissed until you update.
- **Security release notes.** Each security release ships with release notes that call out the fix in general terms. Details stay light on purpose, so churches that haven't updated yet stay protected.
- **Direct communication.** We announce security releases on our blog, on social media, and on the Rockcast. When a fix needs action from you, we say so and explain what to do.

## Safe Harbor for Good-Faith Research

If you make a good-faith effort to follow this policy, we will not pursue legal action against you or refer you to law enforcement for that research.

This protection covers research that:

- Targets only our designated test environments: our [demo site](https://rock.rocksolidchurchdemo.com/) and [pre-alpha site](https://prealpha.rocksolidchurchdemo.com/). It does not cover rockrms.com generally or any other Spark-operated system.
- Stops and reports as soon as a vulnerability is identified, rather than digging further or expanding scope
- Avoids exfiltrating data beyond what's needed to demonstrate the issue, and never modifies or deletes data
- Avoids denial-of-service testing, social engineering of staff, or physical attacks on facilities
- Is reported privately and promptly through Talos, rather than disclosed publicly first

**This does not cover a church's own self-hosted Rock instance.** Most Rock installations are run independently by individual churches. Their systems and data belong to them, not to Spark, and we can't authorize research against a system we don't control.

## Shared Responsibility

Because most installs are self-hosted, security is a partnership between Spark Development Network and the organization running each instance.

**Spark Development Network**

- Fixing vulnerabilities in the Rock RMS codebase
- Communicating fixes clearly and promptly
- Alerting administrators inside Rock when they are on an unsecured version
- Maintaining this coordinated disclosure process
- Following secure software development practices

**Your organization**

- Keeping Rock updated to a supported, patched version
- Securing your server, network, and hosting environment
- Managing user permissions and account access within Rock
- Avoiding cloned blocks, which can't receive our security fixes
- Turning on powerful features, like Lava SQL, only when you need them
- Following Spark's best practices for running Rock

## Common Questions

**Why doesn't Rock publish CVEs?**

CVE numbers help most when a product sits deep in a software stack, and its users have no direct line to the maker. Rock sits at the top of the stack, and we talk to our community directly. We use our own channels instead: an unmissable banner in Rock, security release notes, and direct communication. Getting a CVE number can also take weeks, and we don't want to hold up a fix to wait for one.

**Will you ever publish CVEs?**

We aren't against them. If a good reason comes up, we will reconsider.

**How will I know if I'm on an unsecured version?**

Rock administrators see a red banner on the internal homepage. You can't dismiss it until you update.

**Why are your security updates so general?**

Some churches update slowly, and detailed write-ups would put those churches at risk. We share enough information to act on and no more.

**Do I have to update right away?**

We strongly recommend it. Our security releases contain security fixes only, and we work hard to make them safe to install.

Thank you for helping keep Rock RMS secure.
