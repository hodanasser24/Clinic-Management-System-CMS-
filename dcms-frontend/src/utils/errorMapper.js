export const getFriendlyErrorMessage = (error) => {
  if (!error) return "An unknown error occurred.";
  if (typeof error === "string") return error;
  if (error.response && error.response.data) {
    if (typeof error.response.data === "string") return error.response.data;
    if (error.response.data.message) return error.response.data.message;
    if (error.response.data.title) return error.response.data.title;
    if (error.response.data.errors) {
      const messages = Object.values(error.response.data.errors).flat();
      if (messages.length > 0) return messages[0];
    }
  }
  if (error.message) return error.message;
  return "An unexpected error occurred. Please try again.";
};
