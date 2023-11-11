terraform {
  cloud {
    organization = "DayCare"

    workspaces {
      project = "DayCare"
      name = "DayCare"
    }
  }
}