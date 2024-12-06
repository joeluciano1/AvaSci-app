# Load required libraries
library(ggplot2)
library(gridExtra)  # For combining plots into one image

# Function to install missing packages
install_if_missing <- function(packages) {
  for (pkg in packages) {
    if (!require(pkg, character.only = TRUE)) {
      install.packages(pkg, repos = "https://cloud.r-project.org")
      library(pkg, character.only = TRUE)
    }
  }
}

# Check and install required packages
install_if_missing(c("ggplot2", "gridExtra"))

# Parse command-line arguments
args <- commandArgs(trailingOnly = TRUE)
if (length(args) < 2) {
  stop("Error: Missing arguments. Usage: Rscript generate_report.R <csv_path> <output_path>")
}

csv_path <- normalizePath(args[1])  # Path to the input CSV file
output_path <- normalizePath(args[2])  # Path to save the output files

# Ensure output directory exists
if (!dir.exists(output_path)) {
  dir.create(output_path, recursive = TRUE)
}

# Load the CSV file
data <- read.csv(csv_path, check.names = FALSE)  # Preserve column names with spaces

# Debugging: Print column names and first few rows
cat("Column names in the dataset:\n")
print(names(data))
cat("First few rows of the dataset:\n")
print(head(data))

# Define columns for plotting
x_column <- "Name Of Reading"  # Column for the x-axis
value_columns <- c("Mini Value", "Max Value", "Range")  # Columns for y-axis

# Initialize a list to store plots
plots <- list()

# Generate a plot for each value column
for (col in value_columns) {
  # Check if the column exists in the data
  if (!col %in% names(data)) {
    cat(paste("Column", col, "not found in the dataset. Skipping.\n"))
    next
  }
  
  # Create the plot
  plot <- ggplot(data, aes(x = .data[[x_column]], y = .data[[col]])) +
          geom_bar(stat = "identity", fill = "blue") +
          ggtitle(paste(col, "by Reading")) +
          xlab("Reading") +
          ylab(col) +
          theme_minimal() +
          theme(axis.text.x = element_text(angle = 45, hjust = 1))  # Rotate x-axis labels for readability
  
  # Add the plot to the list
  plots[[col]] <- plot
}

# Combine all plots into a single PNG file
if (length(plots) > 0) {
  combined_plot_file <- file.path(output_path, "combined_report_plots.png")
  
  # Arrange plots in a vertical grid (one column)
  combined_plot <- arrangeGrob(grobs = plots, ncol = 1)
  
  # Save the combined plot as a single PNG
  ggsave(combined_plot_file, combined_plot, width = 8, height = 6 * length(plots))
  
  cat("Saved combined plot to:", combined_plot_file, "\n")
} else {
  cat("No plots generated to combine.\n")
}

# Save a log file as confirmation
log_file <- file.path(output_path, "report_log.txt")
log_messages <- c(
  "Report generated successfully!",
  paste("CSV File:", csv_path),
  paste("Plots generated:", paste(names(plots), collapse = ", "))
)
if (length(plots) > 0) {
  log_messages <- c(log_messages, paste("Combined plot file:", combined_plot_file))
}
writeLines(log_messages, log_file)

# Print success message to console
cat("All plots combined and saved successfully!\n")
