# Tenure Information API

Tenure API is the system of record for Hackney housing tenures. It accepts authenticated HTTP requests through API Gateway, reads and writes tenure records in the `TenureInformation` DynamoDB table, and publishes a domain event to the `tenure.fifo` SNS topic for every create or change. The table also has a DynamoDB stream (`NEW_AND_OLD_IMAGES`) that Terraform maps to the `dynamodb-tenure-stream-listener-<environment>` Lambda.

Build, run and test steps are in [CONTRIBUTION.md](CONTRIBUTION.md).

## HTTP endpoints

All routes are under `/api/v1`. API Gateway sends `/{proxy+}` (any method) through the externally managed Lambda authorizer. `/swagger/{proxy+}` (GET) is not authorized.

| Method | Route | Purpose | SNS event published |
| --- | --- | --- | --- |
| GET | `/tenures/{id}` | Get a tenure. Returns an `ETag` header containing the version number. | None |
| POST | `/tenures` | Create a tenure. Returns 201. | `TenureCreatedEvent` |
| PATCH | `/tenures/{id}` | Edit tenure details. Uses `If-Match` for optimistic concurrency (409 on mismatch). Changing `charges` requires a group listed in `EDIT_CHARGES_ALLOWED_GROUPS`. | `TenureUpdatedEvent` |
| PATCH | `/tenures/{id}/person/{personid}` | Add or update a household member on the tenure. Uses `If-Match`. | `PersonAddedToTenureEvent` |
| DELETE | `/tenures/{tenureId}/person/{personId}` | Remove a household member from the tenure. | `PersonRemovedFromTenureEvent` |
| GET | `/healthcheck/ping` | DynamoDB health check | None |

## Environment variables

| Variable | Source in AWS (`serverless.yml`) | Purpose |
| --- | --- | --- |
| `TENURE_SNS_ARN` | SSM `/sns-topic/{stage}/tenure/arn` | Topic that receives tenure events |
| `EDIT_CHARGES_ALLOWED_GROUPS` | SSM `/housing-tl/{stage}/tenure-edit-charges-allowed-groups` | Groups allowed to edit tenure charges |
| `DISALLOWED_EMAIL` | SSM `/housing-tl/{stage}/disallowed-email` | A user email that is blocked from reading one specific tenure (HPT-641) |
| `DynamoDb_LocalMode`, `DynamoDb_LocalServiceUrl` | Local only | Point the app at DynamoDB Local |
| `Sns_LocalMode`, `Localstack_SnsServiceUrl` | Local only | Point the app at LocalStack SNS |

## External dependencies

| Dependency | Use |
| --- | --- |
| DynamoDB table `TenureInformation` (hash key `id`) | Tenure storage |
| SNS topic `tenure.fifo` | Publishes tenure domain events |
| `api-gateway-lambda-authorizer` (one per account) | Request authorizer on API Gateway |

The service makes no outbound HTTP calls to other APIs.

## Stack

| Area | Technology |
| --- | --- |
| Runtime | .NET `net10.0` (ASP.NET Core) on AWS Lambda `dotnet10`, deployed with Serverless Framework |
| AWS | API Gateway (with the `Platform_APIs_Web_ACL` WAF), Lambda in a VPC, DynamoDB, SNS, SSM Parameter Store, X-Ray, CloudWatch Logs |
| Storage | DynamoDB `TenureInformation` |
| Infrastructure | Terraform in `terraform/{development,staging,production}` |
| CI/CD | CircleCI with SonarCloud |

## Contacts

Housing product team: [mmh-project-team@hackney.gov.uk](mailto:mmh-project-team@hackney.gov.uk)
