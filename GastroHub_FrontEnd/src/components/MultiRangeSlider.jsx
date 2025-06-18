import React, { useCallback, useEffect, useState, useRef } from "react";
import PropTypes from "prop-types";
import "./slider.css";

export default function MultiRangeSlider({ min, max, value, onChange }) {
  /* ───────────── utils ───────────── */
  const normalise = useCallback(
    (v) => {
      if (Array.isArray(v)) return [v[0] ?? min, v[1] ?? max];
      if (v && typeof v === "object")
        return [v.min ?? v.minValue ?? min, v.max ?? v.maxValue ?? max];
      return [min, max];
    },
    [min, max]
  );

  /* ───────────── local state ───────────── */
  const [minVal, setMinVal] = useState(() => normalise(value)[0]);
  const [maxVal, setMaxVal] = useState(() => normalise(value)[1]);

  /* keep state in sync **only** when parent really changes the numbers */
  useEffect(() => {
    if (value === undefined) return;
    const [extMin, extMax] = normalise(value);
    setMinVal((cur) => (cur !== extMin ? extMin : cur));
    setMaxVal((cur) => (cur !== extMax ? extMax : cur));
  }, [value]); // `normalise` is memo-stable, so no need to include it

  /* ───────────── track colouring ───────────── */
  const rangeRef = useRef(null);
  const getPercent = useCallback(
    (val) => Math.round(((val - min) / (max - min)) * 100),
    [min, max]
  );

  useEffect(() => {
    if (!rangeRef.current) return;
    const minPct = getPercent(minVal);
    const maxPct = getPercent(maxVal);
    rangeRef.current.style.left = `${minPct}%`;
    rangeRef.current.style.width = `${maxPct - minPct}%`;
  }, [minVal, maxVal, getPercent]);

  /* ───────────── thumb handlers ───────────── */
  const handleMinChange = (e) => {
    const val = Math.min(Number(e.target.value), maxVal - 1);
    setMinVal(val); // local update for immediate UI feedback
    onChange?.([val, maxVal]); // bubble up
  };

  const handleMaxChange = (e) => {
    const val = Math.max(Number(e.target.value), minVal + 1);
    setMaxVal(val);
    onChange?.([minVal, val]);
  };

  /* ───────────── render ───────────── */
  return (
    <div className="container">
      {/* left thumb */}
      <input
        type="range"
        min={min}
        max={max}
        value={minVal}
        onChange={handleMinChange}
        className="thumb thumb--left"
        style={{ zIndex: minVal > max - 100 ? 5 : undefined }}
      />

      {/* right thumb */}
      <input
        type="range"
        min={min}
        max={max}
        value={maxVal}
        onChange={handleMaxChange}
        className="thumb thumb--right"
      />

      {/* track */}
      <div className="slider">
        <div className="slider__track" />
        <div ref={rangeRef} className="slider__range" />
        <div className="slider__left-value">{minVal}</div>
        <div className="slider__right-value">{maxVal}</div>
      </div>
    </div>
  );
}

MultiRangeSlider.propTypes = {
  min: PropTypes.number.isRequired,
  max: PropTypes.number.isRequired,
  /** Accepts [min, max] or {min, max} or {minValue, maxValue} */
  value: PropTypes.oneOfType([
    PropTypes.arrayOf(PropTypes.number),
    PropTypes.shape({
      min: PropTypes.number,
      max: PropTypes.number,
      minValue: PropTypes.number,
      maxValue: PropTypes.number,
    }),
  ]),
  /** Called with `[min, max]` whenever either thumb moves */
  onChange: PropTypes.func,
};
