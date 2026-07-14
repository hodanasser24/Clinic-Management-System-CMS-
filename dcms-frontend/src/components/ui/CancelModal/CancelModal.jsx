import { useState } from "react";
import "./CancelModal.css";

export default function CancelModal({ isOpen, onClose, onConfirm, errorMessage }) {
  const [reason, setReason] = useState("");
  const [error, setError] = useState("");

  if (!isOpen) return null;

  const handleConfirm = () => {
    if (!reason.trim()) {
      setError("Cancellation reason cannot be empty.");
      return;
    }
    setError("");
    onConfirm(reason);
    setReason("");
  };

  const handleClose = () => {
    setError("");
    setReason("");
    onClose();
  };

  const displayError = error || errorMessage;

  return (
    <div className="cancel-modal-overlay">
      <div className="cancel-modal-content">
        <h2>Cancel Appointment</h2>
        <p>Please provide a reason for cancelling this appointment.</p>
        
        <div className="input-group">
          <textarea
            rows="3"
            placeholder="Enter reason..."
            value={reason}
            onChange={(e) => {
              setReason(e.target.value);
              if (error) setError("");
            }}
            className={displayError ? "input-error" : ""}
            autoFocus
          />
          {displayError && <span className="error-text">{displayError}</span>}
        </div>

        <div className="cancel-modal-actions">
          <button className="btn-cancel" onClick={handleClose}>
            Keep Appointment
          </button>
          <button className="btn-confirm danger" onClick={handleConfirm}>
            Cancel Appointment
          </button>
        </div>
      </div>
    </div>
  );
}
