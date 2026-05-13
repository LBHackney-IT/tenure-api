resource "aws_ssm_parameter" "tenure_edit_charges_allowed_groups" {
  name  = "/housing-tl/pre-production/tenure-edit-charges-allowed-groups"
  type  = "String"
  value = "to_be_set_manually"

  lifecycle {
    ignore_changes = [
      value,
    ]
  }
}
