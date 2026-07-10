import { useState, useEffect, useRef } from "react";
import "./DentalChart.css";

const statuses = [
  { name: "Healthy", color: "#22c55e" },
  { name: "Filling", color: "#facc15" },
  { name: "Cavity", color: "#ef4444" },
  { name: "Root Canal", color: "#3b82f6" },
  { name: "Extraction", color: "#94a3b8" },
  { name: "Implant", color: "#a855f7" },
];

const upperTeeth = [
  18, 17, 16, 15, 14, 13, 12, 11, 21, 22, 23, 24, 25, 26, 27, 28,
];
const lowerTeeth = [
  48, 47, 46, 45, 44, 43, 42, 41, 31, 32, 33, 34, 35, 36, 37, 38,
];

// FDI quadrants angles:
// Symmetrical coordinates matching the teeth in the background image
const upperLeftAngles = [184, 171, 158, 145, 132, 119, 107, 95.5];
const upperRightAngles = [84.5, 73, 61, 48, 35, 22, 9, -4];
const upperAngles = [...upperLeftAngles, ...upperRightAngles];

const lowerLeftAngles = [182, 169, 157, 145, 133, 121, 110, 99.5];
const lowerRightAngles = [80.5, 70, 59, 47, 35, 23, 11, -2];
const lowerAngles = [...lowerLeftAngles, ...lowerRightAngles];

// Bidirectional FDI to Universal mapping
const fdiToUniversal = {
  18: 1, 17: 2, 16: 3, 15: 4, 14: 5, 13: 6, 12: 7, 11: 8,
  21: 9, 22: 10, 23: 11, 24: 12, 25: 13, 26: 14, 27: 15, 28: 16,
  38: 17, 37: 18, 36: 19, 35: 20, 34: 21, 33: 22, 32: 23, 31: 24,
  41: 25, 42: 26, 43: 27, 44: 28, 45: 29, 46: 30, 47: 31, 48: 32
};

const getType = (tooth) => {
  if ([11, 12, 21, 22, 31, 32, 41, 42].includes(tooth)) return "incisor";
  if ([13, 23, 33, 43].includes(tooth)) return "canine";
  if ([14, 15, 24, 25, 34, 35, 44, 45].includes(tooth)) return "premolar";
  return "molar";
};

const getToothDisplayDetails = (fdiTooth) => {
  if (!fdiTooth) return { number: "--", quadrant: "", name: "" };

  const uni = fdiToUniversal[fdiTooth];

  // Quadrant name
  let quadrant = "";
  if (fdiTooth >= 11 && fdiTooth <= 18) quadrant = "Upper Right";
  else if (fdiTooth >= 21 && fdiTooth <= 28) quadrant = "Upper Left";
  else if (fdiTooth >= 31 && fdiTooth <= 38) quadrant = "Lower Left";
  else if (fdiTooth >= 41 && fdiTooth <= 48) quadrant = "Lower Right";

  // Tooth name
  let name = "";
  const type = getType(fdiTooth);
  if (type === "incisor") {
    name = [11, 21, 31, 41].includes(fdiTooth) ? "Central Incisor" : "Lateral Incisor";
  } else if (type === "canine") {
    name = "Canine";
  } else if (type === "premolar") {
    name = [14, 24, 34, 44].includes(fdiTooth) ? "First Premolar" : "Second Premolar";
  } else {
    name = [16, 26, 36, 46].includes(fdiTooth)
      ? "First Molar"
      : [17, 27, 37, 47].includes(fdiTooth)
      ? "Second Molar"
      : "Third Molar";
  }

  return { number: uni, quadrant, name };
};

const getToothScale = (fdi) => {
  const type = getType(fdi);
  if (type === "molar") {
    if ([18, 28, 38, 48].includes(fdi)) return 0.88;
    return 1.15;
  }
  if (type === "premolar") return 0.95;
  if (type === "canine") return 1.0;
  if ([11, 21].includes(fdi)) return 1.1; // wide upper incisors
  if ([12, 22].includes(fdi)) return 0.9; // lateral incisors
  return 0.82; // lower incisors
};

// SVG paths for static sidebar/preview teeth
const PREVIEW_TOOTH_GEOMETRY = {
  incisor: {
    crown: "M -13,-7 C -13,-9 13,-9 13,-7 C 14,2 13,11 13,11 C 13,13 -13,13 -13,11 Z",
    basin: "M -9,-3 C -9,-5 9,-5 9,-3 C 10,2 9,7 9,7 C 9,8 -9,8 -9,7 Z",
    collar: "M -16,-10 C -16,-12 16,-12 16,-10 C 17,2 16,14 16,14 C 16,16 -16,16 -16,14 Z",
  },
  canine: {
    crown: "M -15,-8 C -15,-11 0,-15 0,-15 C 0,-15 15,-11 15,-8 C 16,2 14,12 14,12 C 14,14 -14,14 -14,12 Z",
    basin: "M -10,-4 C -10,-6 0,-9 0,-9 C 0,-9 10,-6 10,-4 C 11,2 10,8 10,8 C 10,9 -10,9 -10,8 Z",
    collar: "M -18,-11 C -18,-14 0,-18 0,-18 C 0,-18 18,-14 18,-11 C 19,2 18,15 18,15 C 18,17 -18,17 -18,15 Z",
  },
  premolar: {
    crown: "M -18,-12 C -18,-16 18,-16 18,-12 C 20,-4 19,12 19,12 C 19,16 -19,16 -19,12 Z",
    basin: "M -12,-7 C -12,-10 12,-10 12,-7 C 14,-2 13,7 13,7 C 13,10 -13,10 -13,7 Z",
    collar: "M -22,-15 C -22,-19 22,-19 22,-15 C 24,-4 23,15 23,15 C 23,19 -23,19 -23,15 Z",
  },
  molar: {
    crown: "M -23,-16 C -23,-21 23,-21 23,-16 C 26,-6 24,16 24,16 C 24,21 -24,21 -24,16 C -24,16 -26,-6 -23,-16 Z",
    basin: "M -16,-10 C -16,-13 16,-13 16,-10 C 18,-4 16,10 16,10 C 16,13 -16,13 -16,10 Z",
    collar: "M -27,-20 C -27,-25 27,-25 27,-20 C 30,-6 28,20 28,20 C 28,25 -28,25 -28,20 Z",
  },
};

function DentalChart({ readOnly = false }) {
  const [selectedTooth, setSelectedTooth] = useState(null);
  const [toothStatus, setToothStatus] = useState({
    16: "Filling",
    24: "Cavity",
    36: "Root Canal",
  });

  // Grouped transform state to enforce synchronous batching
  const [transform, setTransform] = useState({ scale: 1, x: 0, y: 0 });
  const [isDragging, setIsDragging] = useState(false);
  const [dragStart, setDragStart] = useState({ x: 0, y: 0 });
  const [isAnimating, setIsAnimating] = useState(false);
  const [spacePressed, setSpacePressed] = useState(false);

  const hasMoved = useRef(false);
  const svgRef = useRef(null);

  const selectedStatus = selectedTooth
    ? toothStatus[selectedTooth] || "Healthy"
    : "No tooth selected";

  const selectedColor =
    statuses.find((s) => s.name === selectedStatus)?.color || "#60a5fa";

  const changeStatus = (status) => {
    if (readOnly || !selectedTooth) return;
    setToothStatus((prev) => ({ ...prev, [selectedTooth]: status }));
  };

  const toothColor = (tooth) => {
    const status = toothStatus[tooth] || "Healthy";
    return statuses.find((s) => s.name === status)?.color || "#22c55e";
  };

  // Coordinates matching the teeth in the background image
  const getToothGeometry = (tooth, index, jaw) => {
    const isUpper = jaw === "upper";
    const angle = isUpper ? upperAngles[index] : lowerAngles[index];

    const cx = 400;
    const cy = isUpper ? 285 : 495;
    const rx = isUpper ? 265 : 245;
    const ry = isUpper ? 175 : 185;

    const angleRad = (angle * Math.PI) / 180;
    const x = cx + rx * Math.cos(angleRad);
    const y = isUpper
      ? cy - ry * Math.sin(angleRad)
      : cy + ry * Math.sin(angleRad);

    const baseRotation = angle - 90;
    const rotation = isUpper ? -0.32 * baseRotation : 0.32 * baseRotation + 180;

    return { x, y, rotation };
  };

  const getLabelCoords = (index, jaw) => {
    const isUpper = jaw === "upper";
    const isLeft = index < 8;
    const labelX = isLeft ? 100 : 700;

    let labelY;
    if (isUpper) {
      labelY = isLeft
        ? 330 - index * 34
        : 92 + (index - 8) * 34;
    } else {
      labelY = isLeft
        ? 450 + index * 34
        : 688 - (index - 8) * 34;
    }
    return { x: labelX, y: labelY };
  };

  // Keyboard Spacebar drag detection
  useEffect(() => {
    const handleKeyDown = (e) => {
      if (e.code === "Space") {
        setSpacePressed(true);
        if (e.target === document.body) {
          e.preventDefault();
        }
      }
    };

    const handleKeyUp = (e) => {
      if (e.code === "Space") {
        setSpacePressed(false);
      }
    };

    window.addEventListener("keydown", handleKeyDown);
    window.addEventListener("keyup", handleKeyUp);
    return () => {
      window.removeEventListener("keydown", handleKeyDown);
      window.removeEventListener("keyup", handleKeyUp);
    };
  }, []);

  // Mouse pan/drag handlers - Disabled when scale is 1.0 (locked)
  const handleMouseDown = (e) => {
    if (e.button !== 0) return; // Left click only
    if (transform.scale <= 1.0) return; // Locked at scale <= 1.0

    setIsDragging(true);
    setDragStart({ x: e.clientX, y: e.clientY });
    hasMoved.current = false;
  };

  const handleMouseMove = (e) => {
    if (!isDragging) return;
    if (transform.scale <= 1.0) return; // Locked at scale <= 1.0

    const dx = e.clientX - dragStart.x;
    const dy = e.clientY - dragStart.y;

    if (Math.abs(dx) > 2 || Math.abs(dy) > 2) {
      hasMoved.current = true;
    }

    setTransform((prev) => ({
      ...prev,
      x: prev.x + dx,
      y: prev.y + dy
    }));
    setDragStart({ x: e.clientX, y: e.clientY });
  };

  const handleMouseUp = () => {
    setIsDragging(false);
  };

  const handleToothClick = (tooth) => {
    if (hasMoved.current) return; // Prevent selection if mouse was dragged
    setSelectedTooth(tooth);
  };

  // Smooth reset helper centered on viewBox middle
  const triggerSmoothReset = () => {
    setIsAnimating(true);
    setTransform({ scale: 1, x: 0, y: 0 });
    setTimeout(() => setIsAnimating(false), 300);
  };

  // Smooth Zoom Centered on the center of the chart (400, 390)
  const zoomCentered = (zoomFactor) => {
    setIsAnimating(true);
    const cx = 400;
    const cy = 390;

    setTransform((prev) => {
      const newScale = Math.min(Math.max(prev.scale * zoomFactor, 1.0), 4.0);
      
      if (newScale <= 1.0) {
        return { scale: 1.0, x: 0, y: 0 };
      }

      const newX = cx - (cx - prev.x) * (newScale / prev.scale);
      const newY = cy - (cy - prev.y) * (newScale / prev.scale);
      return { scale: newScale, x: newX, y: newY };
    });

    setTimeout(() => setIsAnimating(false), 300);
  };

  const handleZoomIn = () => {
    zoomCentered(1.2);
  };

  const handleZoomOut = () => {
    zoomCentered(1 / 1.2);
  };

  const renderToothPointerLine = (tooth, index, jaw) => {
    const { x: toothX, y: toothY } = getToothGeometry(tooth, index, jaw);
    const { x: labelX, y: labelY } = getLabelCoords(index, jaw);

    const selected = selectedTooth === tooth;
    const color = toothColor(tooth);

    return (
      <line
        key={`line-${tooth}`}
        x1={labelX}
        y1={labelY}
        x2={toothX}
        y2={toothY}
        className={`label-pointer-line ${selected ? "selected" : ""}`}
        style={selected ? { "--tooth-color": color } : {}}
      />
    );
  };

  const renderToothLabelBadge = (tooth, index, jaw) => {
    const { x: labelX, y: labelY } = getLabelCoords(index, jaw);
    const selected = selectedTooth === tooth;
    const color = toothColor(tooth);
    const uniNum = fdiToUniversal[tooth];

    return (
      <g
        key={`badge-${tooth}`}
        className={`label-badge ${selected ? "selected" : ""}`}
        transform={`translate(${labelX}, ${labelY})`}
        onClick={() => handleToothClick(tooth)}
        style={selected ? { "--tooth-color": color } : {}}
      >
        <rect
          x="-16"
          y="-11"
          width="32"
          height="22"
          rx="5"
          className="label-rect"
        />
        <text x="0" y="0" className="label-text">
          {uniNum}
        </text>
      </g>
    );
  };

  // Render static detailed 3D preview of tooth inside the sidebar
  const renderToothDetailsPreview = (type, status) => {
    const geo = PREVIEW_TOOTH_GEOMETRY[type];
    const isExtracted = status === "Extraction";
    const isImplant = status === "Implant";
    const enamelFill = isImplant ? "url(#implantMetal)" : "url(#toothEnamel)";

    return (
      <>
        <path className="tooth-gum-collar" d={geo.collar} />
        {!isExtracted && (
          <path className="tooth-crown" d={geo.crown} style={{ fill: enamelFill }} />
        )}
        {isExtracted && <path className="tooth-crown-ghost" d={geo.crown} />}
        {!isExtracted && !isImplant && <path className="tooth-basin" d={geo.basin} />}
        {!isExtracted && !isImplant && (
          <g className="cusp-highlights">
            {type === "molar" && (
              <>
                <path d="M -20,-13 C -17,-18 -10,-18 -8,-12 C -9,-7 -16,-6 -20,-13 Z" fill="url(#cuspHighlight)" />
                <path d="M 20,-13 C 17,-18 10,-18 8,-12 C 9,-7 16,-6 20,-13 Z" fill="url(#cuspHighlight)" />
                <path d="M -20,13 C -17,18 -10,18 -8,12 C -9,7 -16,6 -20,13 Z" fill="url(#cuspHighlight)" />
                <path d="M 20,13 C 17,18 10,18 8,12 C 9,7 16,6 20,13 Z" fill="url(#cuspHighlight)" />
              </>
            )}
            {type === "premolar" && (
              <>
                <path d="M -14,-7 C -10,-12 10,-12 14,-7 C 10,-3 -10,-3 -14,-7 Z" fill="url(#cuspHighlight)" />
                <path d="M -14,7 C -10,12 10,12 14,7 C 10,3 -10,3 -14,7 Z" fill="url(#cuspHighlight)" />
              </>
            )}
            {type === "canine" && <polygon points="0,-12 -8,-3 8,-3" fill="url(#cuspHighlight)" />}
            {type === "incisor" && <line x1="-11" y1="-5" x2="11" y2="-5" stroke="#ffffff" strokeWidth="2" opacity="0.85" />}
          </g>
        )}
        {!isExtracted && !isImplant && (
          <g>
            {type === "molar" && <path d="M -15,0 Q 0,-2 15,0 M 0,-10 Q -2,0 0,10" className="tooth-groove" />}
            {type === "premolar" && <path d="M -12,0 C 0,-1.5 0,-1.5 12,0 M 0,-5 L 0,5" className="tooth-groove" />}
            {type === "canine" && <path d="M -10,0 Q 0,3 10,0" className="tooth-groove" opacity="0.5" />}
            {type === "incisor" && <line x1="-11" y1="0" x2="11" y2="0" className="tooth-groove" opacity="0.45" />}
          </g>
        )}
      </>
    );
  };

  // Render transparent interactive hotspots exactly over the background image teeth
  const renderToothHotspot = (tooth, index, jaw) => {
    const status = toothStatus[tooth] || "Healthy";
    const color = toothColor(tooth);
    const selected = selectedTooth === tooth;

    const { x, y, rotation } = getToothGeometry(tooth, index, jaw);
    const type = getType(tooth);
    const scale = getToothScale(tooth);

    const baseRx = type === "molar" ? 21 : type === "premolar" ? 17 : type === "canine" ? 15 : 13;
    const baseRy = type === "molar" ? 17 : type === "premolar" ? 14 : type === "canine" ? 15 : 12;

    const rx = baseRx * scale;
    const ry = baseRy * scale;

    const isExtracted = status === "Extraction";
    const isImplant = status === "Implant";
    const hasRootCanal = status === "Root Canal";
    const hasFilling = status === "Filling";
    const hasCavity = status === "Cavity";

    return (
      <g
        key={tooth}
        className={`tooth-hotspot ${type} ${selected ? "selected" : ""} status-${status.toLowerCase().replace(" ", "-")}`}
        transform={`translate(${x}, ${y}) rotate(${rotation})`}
        onClick={() => handleToothClick(tooth)}
        style={{ "--tooth-color": color }}
      >
        {/* Glowing selected aura */}
        {selected && (
          <ellipse
            cx="0"
            cy="0"
            rx={rx + 4}
            ry={ry + 4}
            fill="none"
            stroke={color}
            strokeWidth="3"
            opacity="0.7"
            filter="url(#blurSoft)"
          />
        )}

        {/* Base Interactive Hotspot Area */}
        <ellipse
          cx="0"
          cy="0"
          rx={rx}
          ry={ry}
          className="hotspot-trigger"
          fill={
            selected
              ? color
              : isExtracted
              ? "rgba(15, 23, 42, 0.75)"
              : isImplant
              ? "rgba(168, 85, 247, 0.25)"
              : hasRootCanal
              ? "rgba(59, 130, 246, 0.25)"
              : hasFilling
              ? "rgba(245, 158, 11, 0.25)"
              : hasCavity
              ? "rgba(239, 68, 68, 0.25)"
              : "rgba(255, 255, 255, 0.001)"
          }
          stroke={
            selected
              ? color
              : isExtracted
              ? "#64748b"
              : isImplant
              ? "#a855f7"
              : hasRootCanal
              ? "#3b82f6"
              : hasFilling
              ? "#facc15"
              : hasCavity
              ? "#ef4444"
              : "rgba(255, 255, 255, 0)"
          }
          strokeWidth={selected || status !== "Healthy" ? 2 : 1}
          style={selected ? { mixBlendMode: "color-dodge" } : {}}
        />

        {/* Graphical status overlays */}
        {isExtracted && (
          <path
            d={`M -${rx * 0.55},-${ry * 0.55} L ${rx * 0.55},${ry * 0.55} M ${rx * 0.55},-${ry * 0.55} L -${rx * 0.55},${ry * 0.55}`}
            stroke="#94a3b8"
            strokeWidth="2.5"
            strokeLinecap="round"
          />
        )}

        {isImplant && (
          <circle cx="0" cy="0" r="3.5" fill="#27272a" stroke="rgba(255, 255, 255, 0.3)" strokeWidth="1" />
        )}

        {hasRootCanal && (
          <circle cx="0" cy="0" r="3.5" fill="#3b82f6" stroke="#1d4ed8" strokeWidth="0.8" />
        )}

        {hasFilling && (
          <path
            d={`M -${rx * 0.45},0 Q 0,-3.5 ${rx * 0.45},0 Q 0,4.5 -${rx * 0.45},0`}
            fill="#f59e0b"
            stroke="#d97706"
            strokeWidth="0.8"
          />
        )}

        {hasCavity && (
          <circle cx="3" cy="2" r="4.5" fill="#ef4444" stroke="#991b1b" strokeWidth="0.8" filter="url(#blurSoft)" />
        )}
      </g>
    );
  };

  const getContainerCursor = () => {
    if (transform.scale <= 1.0) return "default";
    if (spacePressed) {
      return isDragging ? "grabbing" : "grab";
    }
    return isDragging ? "grabbing" : "default";
  };

  return (
    <div className="dental-chart-pro">
      <div className="chart-top">
        <div className="chart-title">
          <div className="chart-logo">
            <svg
              width="26"
              height="26"
              viewBox="0 0 24 24"
              fill="none"
              stroke="#60a5fa"
              strokeWidth="2.2"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <path d="M12 2C9.5 2 7 3.5 7 7C7 11 9 12 10.5 13.5C11.5 14.5 11.5 16 10 18C8.5 20 9 22 12 22C15 22 15.5 20 14 18C12.5 16 12.5 14.5 13.5 13.5C15 12 17 11 17 7C17 3.5 14.5 2 12 2Z" />
            </svg>
          </div>
          <div>
            <h1>Dental Chart</h1>
            <p>Patient: Sarah Mohamed</p>
            <span>ID: DCMS-2025-00125</span>
          </div>
        </div>

        <div className="chart-actions-top">
          <button className="header-dropdown-btn">
            Permanent Dentition
            <svg
              width="12"
              height="12"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2.5"
              style={{ marginLeft: "8px" }}
            >
              <polyline points="6 9 12 15 18 9" />
            </svg>
          </button>
          <button className="header-history-btn">
            <svg
              width="14"
              height="14"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2.5"
              style={{ marginRight: "8px", verticalAlign: "middle" }}
            >
              <path d="M12 8v4l3 3" />
              <path d="M3.05 11a9 9 0 1 1 .45 4m-.45-4H3v3" />
            </svg>
            Treatment History
          </button>
          <button className="header-menu-btn">
            <svg
              width="16"
              height="16"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2.5"
            >
              <circle cx="12" cy="12" r="1.5" />
              <circle cx="12" cy="5" r="1.5" />
              <circle cx="12" cy="19" r="1.5" />
            </svg>
          </button>
        </div>
      </div>

      <div className="chart-layout">
        <aside className="chart-sidebar">
          <h3>Selected Tooth</h3>
          <div className="selected-tooth-card">
            <div className="big-tooth">
              {selectedTooth ? (
                <svg
                  viewBox="-25 -45 50 90"
                  style={{
                    width: "80px",
                    height: "80px",
                    overflow: "visible",
                    "--tooth-color": selectedColor,
                  }}
                  className="sidebar-svg"
                >
                  {renderToothDetailsPreview(getType(selectedTooth), selectedStatus)}
                </svg>
              ) : (
                <span className="emoji-fallback">🦷</span>
              )}
            </div>
            {selectedTooth && (
              <div className="selected-tooth-details">
                <div className="selected-number">{getToothDisplayDetails(selectedTooth).number}</div>
                <p className="tooth-name">{getToothDisplayDetails(selectedTooth).quadrant}</p>
                <p className="tooth-type">{getToothDisplayDetails(selectedTooth).name}</p>
              </div>
            )}
          </div>

          <div className="divider"></div>

          <h4>Current Status</h4>
          <div
            className="current-status"
            style={{
              background: `${selectedColor}15`,
              border: `1px solid ${selectedColor}35`,
              color: selectedColor,
              "--status-color": selectedColor,
            }}
          >
            <span></span>
            {selectedStatus}
          </div>

          <div className="divider"></div>

          <h4>Notes</h4>
          <div className="notes-box">
            <span>No notes added</span>
            {!readOnly && (
              <button type="button" className="notes-edit-btn">
                <svg
                  width="14"
                  height="14"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2.5"
                >
                  <path d="M12 20h9M16.5 3.5a2.12 2.12 0 0 1 3 3L7 19l-4 1 1-4Z" />
                </svg>
              </button>
            )}
          </div>

          {!readOnly && (
            <>
              <div className="divider"></div>

              <h4>Change Status</h4>
              <div className="status-list">
                {statuses.map((status) => {
                  const isSelected = selectedStatus === status.name;
                  return (
                    <button
                      key={status.name}
                      type="button"
                      onClick={() => changeStatus(status.name)}
                      className={isSelected ? "selected-status" : ""}
                      style={
                        isSelected
                          ? {
                              borderColor: `${status.color}70`,
                              background: `${status.color}15`,
                              color: status.color,
                              boxShadow: `0 0 10px ${status.color}25`,
                            }
                          : {}
                      }
                    >
                      <span style={{ background: status.color }}></span>
                      {status.name}
                    </button>
                  );
                })}
              </div>

              <button className="save-btn">
                <svg
                  width="16"
                  height="16"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2.5"
                  style={{ marginRight: "8px", verticalAlign: "middle" }}
                >
                  <path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z" />
                  <polyline points="17 21 17 13 7 13 7 21" />
                  <polyline points="7 3 7 8 15 8" />
                </svg>
                Save Changes
              </button>
            </>
          )}
        </aside>

        <section
          className={`svg-chart-area ${spacePressed ? "space-pressed" : ""}`}
          style={{ cursor: getContainerCursor() }}
          onMouseDown={handleMouseDown}
          onMouseMove={handleMouseMove}
          onMouseUp={handleMouseUp}
          onMouseLeave={handleMouseUp}
        >
          <svg
            ref={svgRef}
            viewBox="0 0 800 780"
            className="dental-svg"
            onDoubleClick={triggerSmoothReset}
          >
            <defs>
              {/* CAD grid pattern overlay */}
              <pattern
                id="grid"
                width="30"
                height="30"
                patternUnits="userSpaceOnUse"
              >
                <path
                  d="M 30 0 L 0 0 0 30"
                  fill="none"
                  stroke="rgba(59, 130, 246, 0.02)"
                  strokeWidth="1"
                />
              </pattern>

              {/* Soft blur filter for glows */}
              <filter id="blurSoft" x="-50%" y="-50%" width="200%" height="200%">
                <feGaussianBlur stdDeviation="2.5" />
              </filter>

              {/* Tooth 3D Radial gradients (enamel highlights) for sidebar preview */}
              <radialGradient id="toothEnamel" cx="30%" cy="30%" r="70%">
                <stop offset="0%" stopColor="#fffaec" />
                <stop offset="25%" stopColor="#f5efe0" />
                <stop offset="75%" stopColor="#dfd5b8" />
                <stop offset="100%" stopColor="#c5b692" />
              </radialGradient>

              <radialGradient id="cuspHighlight" cx="50%" cy="50%" r="50%">
                <stop offset="0%" stopColor="#ffffff" stopOpacity="0.9" />
                <stop offset="60%" stopColor="#f5efe0" stopOpacity="0.4" />
                <stop offset="100%" stopColor="#f5efe0" stopOpacity="0" />
              </radialGradient>

              {/* Titanium Implant metallic gradient */}
              <linearGradient id="implantMetal" x1="0%" y1="0%" x2="100%" y2="100%">
                <stop offset="0%" stopColor="#7a8294" />
                <stop offset="40%" stopColor="#b4bece" />
                <stop offset="70%" stopColor="#636b7c" />
                <stop offset="100%" stopColor="#3b414d" />
              </linearGradient>

              {/* Upper Gum Palate Radial Gradient */}
              <radialGradient id="upperGumGradient" cx="50%" cy="30%" r="70%">
                <stop offset="0%" stopColor="#e88478" />
                <stop offset="45%" stopColor="#cc5549" />
                <stop offset="80%" stopColor="#962d22" />
                <stop offset="100%" stopColor="#5c1209" />
              </radialGradient>
            </defs>

            {/* Zoom and Pan group container */}
            <g
              transform={`translate(${transform.x}, ${transform.y}) scale(${transform.scale})`}
              style={
                isAnimating
                  ? { transition: "transform 300ms cubic-bezier(0.25, 0.8, 0.25, 1)" }
                  : {}
              }
            >
              {/* Grid background overlay */}
              <rect width="100%" height="100%" fill="url(#grid)" />

              {/* Centered realistic 3D anatomical background image */}
              <image
                href="/dental-jaw.jpg"
                x="120"
                y="30"
                width="560"
                height="720"
                preserveAspectRatio="xMidYMid slice"
              />

              {/* Center line separator */}
              <line
                x1="120"
                y1="390"
                x2="680"
                y2="390"
                className="chart-separator-line"
              />
              <circle cx="400" cy="390" r="4" className="chart-separator-dot" />

              {/* UPPER TEETH POINTER LINES (rendered behind hot-spots) */}
              {upperTeeth.map((tooth, index) =>
                renderToothPointerLine(tooth, index, "upper"),
              )}

              {/* UPPER TEETH HOTSPOTS (overlayed precisely over background image teeth) */}
              {upperTeeth.map((tooth, index) =>
                renderToothHotspot(tooth, index, "upper"),
              )}

              {/* UPPER TEETH LABEL BADGES (rendered on top) */}
              {upperTeeth.map((tooth, index) =>
                renderToothLabelBadge(tooth, index, "upper"),
              )}

              {/* Upper Jaw Title inside Arch */}
              <text x="140" y="130" className="jaw-title-fixed left-aligned">
                Upper Jaw
              </text>

              {/* LOWER TEETH POINTER LINES (rendered behind hot-spots) */}
              {lowerTeeth.map((tooth, index) =>
                renderToothPointerLine(tooth, index, "lower"),
              )}

              {/* LOWER TEETH HOTSPOTS (overlayed precisely over background image teeth) */}
              {lowerTeeth.map((tooth, index) =>
                renderToothHotspot(tooth, index, "lower"),
              )}

              {/* LOWER TEETH LABEL BADGES (rendered on top) */}
              {lowerTeeth.map((tooth, index) =>
                renderToothLabelBadge(tooth, index, "lower"),
              )}

              {/* Lower Jaw Title inside Arch */}
              <text x="400" y="440" className="jaw-title-fixed centered">
                Lower Jaw
              </text>
            </g>
          </svg>

          {/* Floating Dedicated Zoom Widget (matches zoom-only request) */}
          <div className="chart-controls-float zoom-only-widget">
            <button
              type="button"
              className="control-btn"
              onClick={handleZoomIn}
              title="Zoom In"
            >
              <svg
                width="14"
                height="14"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2.5"
              >
                <line x1="12" y1="5" x2="12" y2="19" />
                <line x1="5" y1="12" x2="19" y2="12" />
              </svg>
            </button>
            <div
              className="zoom-percent-display"
              onClick={triggerSmoothReset}
              title="Click to reset zoom (100%)"
            >
              {Math.round(transform.scale * 100)}%
            </div>
            <button
              type="button"
              className="control-btn"
              onClick={handleZoomOut}
              title="Zoom Out"
            >
              <svg
                width="14"
                height="14"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2.5"
              >
                <line x1="5" y1="12" x2="19" y2="12" />
              </svg>
            </button>
          </div>
        </section>
      </div>

      <div className="chart-legend-pro">
        {statuses.map((status) => (
          <div key={status.name}>
            <span style={{ background: status.color }}></span>
            {status.name}
          </div>
        ))}
      </div>
    </div>
  );
}

export default DentalChart;
