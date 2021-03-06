resource "aws_dynamodb_table" "tenureinformationapi_dynamodb_table" {
  name           = "TenureInformation"
  billing_mode   = "PROVISIONED"
  read_capacity  = 10
  write_capacity = 10
  hash_key       = "id"

  attribute {
    name = "id"
    type = "S"
  }

  tags = merge(
    local.default_tags,
    { BackupPolicy = "Stg" }
  )

  point_in_time_recovery {
    enabled = true
  }
}