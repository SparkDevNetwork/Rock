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
2. **We assign a CVE early.** Once we've confirmed and reproduced the issue, we request a CVE via GitHub and share it with the reporter right away. The advisory itself stays private until day 90.
3. **We fix and patch.** We develop and test the fix in a private fork, then ship it in a scheduled release, or in an emergency patch for severe issues or environments under active attack.
4. **We publish 90 days later.** Advisories go live on [GitHub Security Advisories](https://github.com/SparkDevNetwork/Rock/security/advisories) 90 days after release. They're general in detail, giving churches time to update.

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
- Maintaining this coordinated disclosure process
- Following secure software development practices

**Your organization**

- Keeping Rock updated to a supported, patched version
- Securing your server, network, and hosting environment
- Managing user permissions and account access within Rock
- Following Spark's best practices for running Rock

Thank you for helping keep Rock RMS secure.
